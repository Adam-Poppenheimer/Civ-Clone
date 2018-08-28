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
            var allSections = landSections.Concat(waterSections).ToList();

            var homeCentroid = allSections.Select(section => section.Centroid)
                                          .Aggregate((sum, nextCentroid) => sum + nextCentroid) / allSections.Count;

            var southwestLand = landSections.Where(section => section.Centroid.x <= homeCentroid.x && section.Centroid.z <= homeCentroid.z);
            var southeastLand = landSections.Where(section => section.Centroid.x >  homeCentroid.x && section.Centroid.z <= homeCentroid.z);
            var northwestLand = landSections.Where(section => section.Centroid.x <= homeCentroid.x && section.Centroid.z >  homeCentroid.z);
            var northeastLand = landSections.Where(section => section.Centroid.x >  homeCentroid.x && section.Centroid.z >  homeCentroid.z);

            var southwestWater = waterSections.Where(section => section.Centroid.x <= homeCentroid.x && section.Centroid.z <= homeCentroid.z);
            var southeastWater = waterSections.Where(section => section.Centroid.x >  homeCentroid.x && section.Centroid.z <= homeCentroid.z);
            var northwestWater = waterSections.Where(section => section.Centroid.x <= homeCentroid.x && section.Centroid.z >  homeCentroid.z);
            var northeastWater = waterSections.Where(section => section.Centroid.x >  homeCentroid.x && section.Centroid.z >  homeCentroid.z);

            var regions = new List<MapRegion>() {
                new MapRegion(southeastLand.SelectMany(section => section.Cells).ToList(), southeastWater.SelectMany(section => section.Cells).ToList()),
                new MapRegion(southwestLand.SelectMany(section => section.Cells).ToList(), southwestWater.SelectMany(section => section.Cells).ToList()),
                new MapRegion(northeastLand.SelectMany(section => section.Cells).ToList(), northeastWater.SelectMany(section => section.Cells).ToList()),
                new MapRegion(northwestLand.SelectMany(section => section.Cells).ToList(), northwestWater.SelectMany(section => section.Cells).ToList()),
            };

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

        #endregion
        
    }

}
