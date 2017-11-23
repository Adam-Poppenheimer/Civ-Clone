using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using BetterUI;

using Assets.UI;
using Assets.Cities.Buildings;

namespace Assets.Cities.UI {

    public class CityDisplay : UIPanel {

        #region instance fields and properties

        [SerializeField] private Text UnemployedPeopleField;

        private ICity CityToDisplay;

        private ICityTileDisplay TileDisplay;

        private IProductionDisplay ProductionDisplay;

        private ICityGrowthDisplay GrowthDisplay;

        private ICityEventBroadcaster CityEventBroadcaster;

        private IResourceSummaryDisplay YieldDisplay;

        private IResourceGenerationLogic ResourceGenerationLogic;

        private ICityBuildingDisplay BuildingDisplay;

        private IBuildingPossessionCanon BuildingPossessionCanon;

        private ITilePossessionCanon TilePossessionCanon;

        private ICityExpansionDisplay CityExpansionDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICityTileDisplay tileDisplay, ICityEventBroadcaster cityEventBroadcaster,
            IProductionDisplay productionDisplay, ICityGrowthDisplay growthDisplay,
            [Inject(Id = "City Yield Display")] IResourceSummaryDisplay yieldDisplay,
            IResourceGenerationLogic resourceGenerationLogic, ICityBuildingDisplay buildingDisplay,
            IBuildingPossessionCanon buildingPossessionCanon, ITilePossessionCanon tilePossessionCanon,
            ICityExpansionDisplay cityExpansionDisplay
        ){
            TileDisplay             = tileDisplay;
            CityEventBroadcaster    = cityEventBroadcaster;
            ProductionDisplay       = productionDisplay;
            GrowthDisplay           = growthDisplay;
            YieldDisplay            = yieldDisplay;
            ResourceGenerationLogic = resourceGenerationLogic;
            BuildingDisplay         = buildingDisplay;
            BuildingPossessionCanon = buildingPossessionCanon;
            TilePossessionCanon     = tilePossessionCanon;
            CityExpansionDisplay    = cityExpansionDisplay;
        }

        #region from UIPanel

        protected override void DoOnEnable() {
            CityToDisplay = CityEventBroadcaster.LastClickedCity;

            TileDisplay         .CityToDisplay = CityToDisplay;
            ProductionDisplay   .CityToDisplay = CityToDisplay;
            GrowthDisplay       .CityToDisplay = CityToDisplay;
            CityExpansionDisplay.CityToDisplay = CityToDisplay;
        }

        public override void UpdateDisplay() {
            if(CityToDisplay != null) {
                TileDisplay         .Refresh();
                ProductionDisplay   .Refresh();
                GrowthDisplay       .Refresh();
                CityExpansionDisplay.Refresh();

                YieldDisplay.DisplaySummary(ResourceGenerationLogic.GetTotalYieldForCity(CityToDisplay));

                BuildingDisplay.DisplayBuildings(BuildingPossessionCanon.GetBuildingsInCity(CityToDisplay));

                int occupiedSlots = 0;
                foreach(var building in BuildingPossessionCanon.GetBuildingsInCity(CityToDisplay)) {
                    occupiedSlots += building.Slots.Count(slot => slot.IsOccupied);
                }

                occupiedSlots += TilePossessionCanon.GetTilesOfCity(CityToDisplay).Count(tile => tile.WorkerSlot.IsOccupied);
                
                UnemployedPeopleField.text = (CityToDisplay.Population - occupiedSlots).ToString();
            }            
        }

        protected override void DoOnDisable() {
            TileDisplay.CityToDisplay = null;
            ProductionDisplay.CityToDisplay = null;
        }

        #endregion

        #endregion

    }

}
