using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class CivHomelandGenerator : ICivHomelandGenerator {

        #region instance fields and properties

        private IRegionGenerator                 RegionGenerator;
        private ITemplateSelectionLogic          TemplateSelectionLogic;
        private IHexGrid                         Grid;
        private IEnumerable<IResourceDefinition> AvailableResources;
        private List<IBalanceStrategy>           AvailableBalanceStrategies;

        #endregion

        #region constructors

        [Inject]
        public CivHomelandGenerator(
            IRegionGenerator regionGenerator, ITemplateSelectionLogic templateSelectionLogic, IHexGrid grid,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources,
            List<IBalanceStrategy> availableBalanceStrategies
        ) {
            RegionGenerator            = regionGenerator;
            TemplateSelectionLogic     = templateSelectionLogic;
            Grid                       = grid;
            AvailableResources         = availableResources;
            AvailableBalanceStrategies = availableBalanceStrategies;
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
                homelandTemplate.StartingResources, AvailableResources, AvailableBalanceStrategies
            );

            var otherRegions = regions.Where(region => region != startingRegion).ToList();

            var otherData = otherRegions.Select(region => new RegionData(
                TemplateSelectionLogic.GetBiomeForLandRegion   (region, mapTemplate),
                TemplateSelectionLogic.GetTopologyForLandRegion(region, mapTemplate),
                homelandTemplate.OtherResources, AvailableResources, AvailableBalanceStrategies
            )).ToList();

            float mapData = 0.7f;
            foreach(var cell in startingRegion.Cells) {
                cell.SetMapData(mapData);
            }

            foreach(var region in otherRegions) {
                mapData -= 0.15f;
                foreach(var cell in region.Cells) {
                    cell.SetMapData(mapData);
                }
            }

            Grid.GetCellAtLocation(homeCentroid).SetMapData(1f);

            return new CivHomelandData(
                startingRegion, startingData,
                otherRegions,   otherData
            );
        }

        public void GenerateTopologyAndEcology(CivHomelandData homelandData, IMapTemplate mapTemplate) {
            RegionGenerator.GenerateTopologyAndEcology(homelandData.StartingRegion, homelandData.StartingData);

            for(int i = 0; i < homelandData.OtherRegions.Count; i++) {
                RegionGenerator.GenerateTopologyAndEcology(
                    homelandData.OtherRegions[i], homelandData.OtherRegionData[i]
                );
            }
        }

        public void DistributeYieldAndResources(CivHomelandData homelandData, IMapTemplate mapTemplate) {
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
