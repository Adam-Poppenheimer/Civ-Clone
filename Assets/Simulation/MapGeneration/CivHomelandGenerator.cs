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

            var startingRegion = regions.Random();
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

            var chunkQueue = new Queue<List<MapSection>>();

            chunkQueue.Enqueue(allSections);

            while(chunkQueue.Count < chunkCount) {
                var chunkToSplit = chunkQueue.Peek();

                if(chunkToSplit.Count() < 2) {
                    Debug.LogWarning("Failed to get a splittable chunk while creating homeland regions");
                    break;
                }

                chunkQueue.Dequeue();

                var landInChunk = chunkToSplit.Intersect(landSections);

                var landCentroid = landInChunk.Select(section => section.Centroid)
                                              .Aggregate((sum, nextCentroid) => sum + nextCentroid) / landInChunk.Count();

                if(UnityEngine.Random.value <= 0.5f) {
                    var westChunk = chunkToSplit.Where(section => section.Centroid.x <= landCentroid.x).ToList();
                    var eastChunk = chunkToSplit.Where(section => section.Centroid.x >  landCentroid.x).ToList();

                    chunkQueue.Enqueue(westChunk);
                    chunkQueue.Enqueue(eastChunk);
                }else {
                    var southChunk = chunkToSplit.Where(section => section.Centroid.z <= landCentroid.z).ToList();
                    var northChunk = chunkToSplit.Where(section => section.Centroid.z >  landCentroid.z).ToList();

                    chunkQueue.Enqueue(southChunk);
                    chunkQueue.Enqueue(northChunk);
                }
            }

            landChunks  = chunkQueue.Select(chunk => chunk.Intersect(landSections) .ToList()).ToList();
            waterChunks = chunkQueue.Select(chunk => chunk.Intersect(waterSections).ToList()).ToList();
        }

        #endregion
        
    }

}
