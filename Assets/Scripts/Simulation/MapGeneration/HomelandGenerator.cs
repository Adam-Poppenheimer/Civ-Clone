using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;
using UnityCustomUtilities.DataStructures;

namespace Assets.Simulation.MapGeneration {

    public class HomelandGenerator : IHomelandGenerator {

        #region instance fields and properties

        private IRegionGenerator        RegionGenerator;
        private ITemplateSelectionLogic TemplateSelectionLogic;
        private ILuxuryDistributor      LuxuryDistributor;
        private IRiverGenerator         RiverGenerator;
        private IVegetationPainter      VegetationPainter;
        private IHomelandBalancer       HomelandBalancer;
        private IStrategicDistributor   StrategicDistributor;
        private IHexGrid                Grid;

        private List<IBalanceStrategy> AvailableBalanceStrategies;

        #endregion

        #region constructors

        [Inject]
        public HomelandGenerator(
            IRegionGenerator regionGenerator, ITemplateSelectionLogic templateSelectionLogic,
            ILuxuryDistributor luxuryDistributor, IRiverGenerator riverGenerator,
            IVegetationPainter vegetationPainter, List<IBalanceStrategy> availableBalanceStrategies,
            IHomelandBalancer homelandBalancer, IStrategicDistributor strategicDistributor,
            IHexGrid grid
        ) {
            RegionGenerator            = regionGenerator;
            TemplateSelectionLogic     = templateSelectionLogic;
            LuxuryDistributor          = luxuryDistributor;
            RiverGenerator             = riverGenerator;
            AvailableBalanceStrategies = availableBalanceStrategies;
            VegetationPainter          = vegetationPainter;
            HomelandBalancer           = homelandBalancer;
            StrategicDistributor       = strategicDistributor;
            Grid                       = grid;
        }

        #endregion

        #region instance methods

        #region from ICivHomelandGenerator

        public HomelandData GetHomelandData(
            ICivilization civ, List<MapSection> landSections, List<MapSection> waterSections,
            GridPartition partition, IHomelandTemplate homelandTemplate, IMapTemplate mapTemplate
        ) {
            var landChunks  = new List<List<MapSection>>();
            var waterChunks = new List<List<MapSection>>();

            IHexCell seedCentroid = landSections[0].CentroidCell;

            var startingLandSections =
                Grid.GetCellsInRadius(seedCentroid, homelandTemplate.StartingRegionRadius)
                    .Select(cell => partition.GetSectionOfCell(cell))
                    .Distinct()
                    .Intersect(landSections);

            var startingWaterSections =
                Grid.GetCellsInRadius(seedCentroid, homelandTemplate.StartingRegionRadius)
                    .Select(cell => partition.GetSectionOfCell(cell))
                    .Distinct()
                    .Intersect(waterSections);

            var startingRegion = new MapRegion(
                startingLandSections .SelectMany(section => section.Cells).ToList(),
                startingWaterSections.SelectMany(section => section.Cells).ToList()
            );

            foreach(var cell in startingRegion.Cells) {
                cell.SetMapData(1f);
            }

            var unassignedLand  = landSections.Except(startingLandSections);
            var unassignedWater = waterSections.Except(startingWaterSections);

            DivideSectionsIntoRectangularChunks(
                unassignedLand, unassignedWater, homelandTemplate.RegionCount,
                out landChunks, out waterChunks
            );

            var regions = new List<MapRegion>();

            regions.Add(startingRegion);

            for(int i = 0; i < landChunks.Count; i++) {
                var region = new MapRegion(
                    landChunks [i].SelectMany(section => section.Cells).ToList(),
                    waterChunks[i].SelectMany(section => section.Cells).ToList()
                );

                regions.Add(region);
            }

            var startingData = new RegionData(
                TemplateSelectionLogic.GetBiomeForLandRegion   (startingRegion, mapTemplate),
                TemplateSelectionLogic.GetTopologyForLandRegion(startingRegion, mapTemplate),
                AvailableBalanceStrategies
            );

            var otherRegions = regions.Where(region => region != startingRegion).ToList();

            var otherData = otherRegions.Select(region => new RegionData(
                TemplateSelectionLogic.GetBiomeForLandRegion   (region, mapTemplate),
                TemplateSelectionLogic.GetTopologyForLandRegion(region, mapTemplate),
                AvailableBalanceStrategies
            )).ToList();

            return new HomelandData(
                startingRegion, startingData, otherRegions, otherData,
                homelandTemplate.LuxuryResourceData, homelandTemplate.YieldAndResources
            );
        }

        public void GenerateTopologyAndEcology(HomelandData homelandData, IMapTemplate mapTemplate) {
            var regions = homelandData.OtherRegions.ToList();
            regions.Add(homelandData.StartingRegion);

            int riveredCells = 0;

            foreach(var region in regions) {
                var regionData = homelandData.GetDataOfRegion(region);

                RegionGenerator.GenerateTopology(region, regionData.Topology);
                RegionGenerator.PaintTerrain    (region, regionData.Biome);

                riveredCells += Mathf.CeilToInt(region.LandCells.Count * regionData.Biome.RiverPercentage * 0.01f);
            }

            var allLandCells  = regions.SelectMany(region => region.LandCells);
            var allWaterCells = regions.SelectMany(region => region.WaterCells);

            RiverGenerator.CreateRivers(allLandCells, allWaterCells, riveredCells);

            RegionGenerator.AssignFloodPlains(allLandCells);

            foreach(var region in regions) {
                var regionData = homelandData.GetDataOfRegion(region);

                VegetationPainter.PaintVegetation(region, regionData.Biome);
            }
        }

        public void DistributeYieldAndResources(HomelandData homelandData, IMapTemplate mapTemplate) {
            LuxuryDistributor   .DistributeLuxuriesAcrossHomeland  (homelandData);
            StrategicDistributor.DistributeStrategicsAcrossHomeland(homelandData);

            HomelandBalancer.BalanceHomelandYields(homelandData);
        }

        #endregion

        private void DivideSectionsIntoRectangularChunks(
            IEnumerable<MapSection> landSections, IEnumerable<MapSection> waterSections, int chunkCount,
            out List<List<MapSection>> landChunks, out List<List<MapSection>> waterChunks
        ) {
            var allSections = landSections.Concat(waterSections).ToList();

            var chunkQueue = new PriorityQueue<List<MapSection>>();

            chunkQueue.Add(allSections, 100f / allSections.Count);

            while(chunkQueue.Count() < chunkCount) {
                var chunkToSplit = chunkQueue.Peek();

                var landInChunk = chunkToSplit.Intersect(landSections);

                if(landInChunk.Count() < 2) {
                    Debug.LogWarning("Failed to get a splittable chunk while creating homeland regions");
                    break;
                }

                chunkQueue.DeleteMin();

                var landCentroid = landInChunk.Select(section => section.Centroid)
                                              .Aggregate((sum, nextCentroid) => sum + nextCentroid) / landInChunk.Count();

                if(IsWiderThanTall(chunkToSplit)) {
                    var westChunk = chunkToSplit.Where(section => section.Centroid.x <= landCentroid.x).ToList();
                    var eastChunk = chunkToSplit.Where(section => section.Centroid.x >  landCentroid.x).ToList();

                    chunkQueue.Add(westChunk, 100f / westChunk.Count);
                    chunkQueue.Add(eastChunk, 100f / eastChunk.Count);
                }else {
                    var southChunk = chunkToSplit.Where(section => section.Centroid.z <= landCentroid.z).ToList();
                    var northChunk = chunkToSplit.Where(section => section.Centroid.z >  landCentroid.z).ToList();

                    chunkQueue.Add(southChunk, 100f / southChunk.Count);
                    chunkQueue.Add(northChunk, 100f / northChunk.Count);
                }
            }

            landChunks  = chunkQueue.Select(chunk => chunk.Intersect(landSections) .ToList()).ToList();
            waterChunks = chunkQueue.Select(chunk => chunk.Intersect(waterSections).ToList()).ToList();
        }

        private bool IsWiderThanTall(List<MapSection> chunkToSplit) {
            int minXCoord = chunkToSplit.Min(section => section.Cells.Min(cell => HexCoordinates.ToOffsetCoordinateX(cell.Coordinates)));
            int maxXCoord = chunkToSplit.Max(section => section.Cells.Max(cell => HexCoordinates.ToOffsetCoordinateX(cell.Coordinates)));

            int minZCoord = chunkToSplit.Min(section => section.Cells.Min(cell => HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates)));
            int maxZCoord = chunkToSplit.Max(section => section.Cells.Max(cell => HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates)));

            return maxXCoord - minXCoord > maxZCoord - minZCoord;
        }

        #endregion
        
    }

}
