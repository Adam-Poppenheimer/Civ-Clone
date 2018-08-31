using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;
using UnityCustomUtilities.DataStructures;

namespace Assets.Simulation.MapGeneration {

    public class CivHomelandGenerator : ICivHomelandGenerator {

        #region instance fields and properties

        private IRegionGenerator        RegionGenerator;
        private ITemplateSelectionLogic TemplateSelectionLogic;
        private ILuxuryDistributor      LuxuryDistributor;
        private IRiverGenerator         RiverGenerator;
        private IVegetationPainter      VegetationPainter;

        private List<IBalanceStrategy> AvailableBalanceStrategies;

        #endregion

        #region constructors

        [Inject]
        public CivHomelandGenerator(
            IRegionGenerator regionGenerator, ITemplateSelectionLogic templateSelectionLogic,
            ILuxuryDistributor luxuryDistributor, IRiverGenerator riverGenerator,
            IVegetationPainter vegetationPainter, List<IBalanceStrategy> availableBalanceStrategies
        ) {
            RegionGenerator            = regionGenerator;
            TemplateSelectionLogic     = templateSelectionLogic;
            LuxuryDistributor          = luxuryDistributor;
            RiverGenerator             = riverGenerator;
            AvailableBalanceStrategies = availableBalanceStrategies;
            VegetationPainter          = vegetationPainter;
        }

        #endregion

        #region instance methods

        #region from ICivHomelandGenerator

        public CivHomelandData GetHomelandData(
            ICivilization civ, List<MapSection> landSections, List<MapSection> waterSections,
            GridPartition partition, ICivHomelandTemplate homelandTemplate, IMapTemplate mapTemplate
        ) {
            var landChunks  = new List<List<MapSection>>();
            var waterChunks = new List<List<MapSection>>();

            DivideSectionsIntoRectangularChunks(
                landSections, waterSections, homelandTemplate.RegionCount,
                out landChunks, out waterChunks
            );

            var regions = new List<MapRegion>();

            for(int i = 0; i < landChunks.Count; i++) {
                var region = new MapRegion(
                    landChunks [i].SelectMany(section => section.Cells).ToList(),
                    waterChunks[i].SelectMany(section => section.Cells).ToList()
                );

                regions.Add(region);
            }

            var startingRegion = GetRegionNearestToCenter(regions);

            var startingData   = new RegionData(
                TemplateSelectionLogic.GetBiomeForLandRegion   (startingRegion, mapTemplate),
                TemplateSelectionLogic.GetTopologyForLandRegion(startingRegion, mapTemplate),
                homelandTemplate.StartingResources, AvailableBalanceStrategies
            );

            var otherRegions = regions.Where(region => region != startingRegion).ToList();

            var otherData = otherRegions.Select(region => new RegionData(
                TemplateSelectionLogic.GetBiomeForLandRegion   (region, mapTemplate),
                TemplateSelectionLogic.GetTopologyForLandRegion(region, mapTemplate),
                homelandTemplate.OtherResources, AvailableBalanceStrategies
            )).ToList();

            return new CivHomelandData(
                startingRegion, startingData,
                otherRegions,   otherData,
                homelandTemplate.LuxuryResourceData
            );
        }

        public void GenerateTopologyAndEcology(CivHomelandData homelandData, IMapTemplate mapTemplate) {
            var regions = homelandData.OtherRegions.ToList();
            regions.Add(homelandData.StartingRegion);

            int riveredCells = 0;

            foreach(var region in regions) {
                var regionData = homelandData.GetDataOfRegion(region);

                RegionGenerator.GenerateTopology(region, regionData.Topology);
                RegionGenerator.PaintTerrain    (region, regionData.Biome);

                RegionGenerator.AssignFloodPlains(region.LandCells);

                riveredCells += Mathf.CeilToInt(region.LandCells.Count * regionData.Biome.RiverPercentage * 0.01f);
            }

            var allLandCells  = regions.SelectMany(region => region.LandCells);
            var allWaterCells = regions.SelectMany(region => region.WaterCells);

            RiverGenerator.CreateRivers(allLandCells, allWaterCells, riveredCells);

            foreach(var region in regions) {
                var regionData = homelandData.GetDataOfRegion(region);

                VegetationPainter.PaintVegetation(region, regionData.Biome);
            }
        }

        public void DistributeYieldAndResources(CivHomelandData homelandData, IMapTemplate mapTemplate) {
            LuxuryDistributor.DistributeLuxuriesAcrossHomeland(homelandData);

            RegionGenerator.DistributeYieldAndResources(homelandData.StartingRegion, homelandData.StartingData);

            for(int i = 0; i < homelandData.OtherRegions.Count; i++) {
                RegionGenerator.DistributeYieldAndResources(
                    homelandData.OtherRegions[i], homelandData.OtherRegionData[i]
                );
            }
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

                if(chunkToSplit.Count() < 2) {
                    Debug.LogWarning("Failed to get a splittable chunk while creating homeland regions");
                    break;
                }

                chunkQueue.DeleteMin();

                var landInChunk = chunkToSplit.Intersect(landSections);

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

        private MapRegion GetRegionNearestToCenter(IEnumerable<MapRegion> regions) {
            var homelandCentroid = regions.Select(region => region.Centroid)
                                          .Aggregate((total, nextCentroid) => total + nextCentroid);

            homelandCentroid /= regions.Count();

            return regions.Aggregate(delegate(MapRegion currentNearest, MapRegion next) {
                float currentNearestDistance = Vector3.Distance(homelandCentroid, currentNearest.Centroid);
                float nextDistance           = Vector3.Distance(homelandCentroid, next          .Centroid);

                return nextDistance < currentNearestDistance ? next : currentNearest;
            });
        }

        #endregion
        
    }

}
