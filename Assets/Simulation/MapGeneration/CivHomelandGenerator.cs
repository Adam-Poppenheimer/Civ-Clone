using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class CivHomelandGenerator : ICivHomelandGenerator {

        #region instance fields and properties

        private IRegionGenerator        RegionGenerator;
        private ITemplateSelectionLogic TemplateSelectionLogic;

        #endregion

        #region constructors

        [Inject]
        public CivHomelandGenerator(
            IRegionGenerator regionGenerator, ITemplateSelectionLogic templateSelectionLogic
        ) {
            RegionGenerator        = regionGenerator;
            TemplateSelectionLogic = templateSelectionLogic;
        }

        #endregion

        #region instance methods

        #region from ICivHomelandGenerator

        public CivHomelandData GetHomelandData(
            ICivilization civ, List<MapSection> landSections, List<MapSection> waterSections,
            ICivHomelandTemplate homelandTemplate
        ) {
            var startingRegion = new MapRegion(
                landSections.SelectMany(section => section.Cells).ToList(),
                waterSections.SelectMany(section => section.Cells).ToList()
            );

            return new CivHomelandData(startingRegion, new List<MapRegion>());
        }

        public void GenerateTopologyAndEcology(CivHomelandData homelandData) {
            var startingTemplate = TemplateSelectionLogic.GetTemplateForLandRegion(homelandData.StartingRegion);

            RegionGenerator.GenerateTopologyAndEcology(homelandData.StartingRegion, startingTemplate);

            foreach(var region in homelandData.OtherRegions) {
                var template = TemplateSelectionLogic.GetTemplateForLandRegion(region);

                RegionGenerator.GenerateTopologyAndEcology(region, template);
            }
        }

        public void DistributeYieldAndResources(CivHomelandData homelandData) {
            var startingTemplate = TemplateSelectionLogic.GetTemplateForLandRegion(homelandData.StartingRegion);

            RegionGenerator.DistributeYieldAndResources(homelandData.StartingRegion, startingTemplate);

            foreach(var region in homelandData.OtherRegions) {
                var template = TemplateSelectionLogic.GetTemplateForLandRegion(region);

                RegionGenerator.DistributeYieldAndResources(region, template);
            }
        }

        #endregion

        #endregion
        
    }

}
