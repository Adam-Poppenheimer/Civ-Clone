﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using Assets.GameMap;
using Assets.Cities.Buildings;
using Assets.Cities.UI;

namespace Assets.Cities {

    public class City : MonoBehaviour, ICity, IPointerClickHandler {

        #region instance fields and properties

        #region from ICity

        public IMapTile Location { get; set; }

        public int Population {
            get { return _population; }
            set { _population = value; }
        }
        [SerializeField] private int _population;

        public int FoodStockpile {
            get { return _foodStockpile; }
            set { _foodStockpile = value; }
        }
        [SerializeField] private int _foodStockpile;

        public int CultureStockpile {
            get { return _cultureStockpile; }
            set { _cultureStockpile = value; }
        }
        [SerializeField] private int _cultureStockpile;

        public ResourceSummary LastIncome { get; private set; }

        public IProductionProject CurrentProject { get; private set; }

        public DistributionPreferences DistributionPreferences { get; set; }

        public IMapTile TileBeingPursued { get; private set; }

        #endregion

        private IPopulationGrowthLogic GrowthLogic;

        private IProductionLogic ProductionLogic;

        private IResourceGenerationLogic ResourceGenerationLogic;

        private IBorderExpansionLogic ExpansionLogic;

        private ITilePossessionCanon TilePossessionCanon;

        private IWorkerDistributionLogic DistributionLogic;

        private IBuildingPossessionCanon BuildingPossessionCanon;

        private ICityEventBroadcaster EventBroadcaster;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPopulationGrowthLogic growthLogic, IProductionLogic productionLogic, 
            IResourceGenerationLogic resourceGenerationLogic, IBorderExpansionLogic expansionLogic,
            ITilePossessionCanon tilePossessionCanon, IWorkerDistributionLogic distributionLogic,
            IBuildingPossessionCanon buildingPossessionCanon, ICityEventBroadcaster eventBroadcaster
        ){
            GrowthLogic = growthLogic;
            ProductionLogic = productionLogic;
            ResourceGenerationLogic = resourceGenerationLogic;
            ExpansionLogic = expansionLogic;
            TilePossessionCanon = tilePossessionCanon;
            DistributionLogic = distributionLogic;
            BuildingPossessionCanon = buildingPossessionCanon;
            EventBroadcaster = eventBroadcaster;
        }

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            EventBroadcaster.BroadcastCityClicked(this, eventData);
        }

        #endregion

        #region from ICity

        public void SetCurrentProject(IProductionProject project) {
            CurrentProject = project;
        }

        public void PerformGrowth() {
            if(FoodStockpile < 0 && Population > 1) {
                Population--;
                FoodStockpile = GrowthLogic.GetFoodStockpileAfterStarvation(this);

            }else if(FoodStockpile >= GrowthLogic.GetFoodStockpileToGrow(this)) {
                FoodStockpile -= GrowthLogic.GetFoodStockpileSubtractionAfterGrowth(this);
                Population++;
            }
        }

        public void PerformProduction() {
            if(CurrentProject == null) {
                return;
            }

            CurrentProject.Progress += ProductionLogic.GetProductionProgressPerTurnOnProject(this, CurrentProject);

            if(CurrentProject.ProductionToComplete >= CurrentProject.Progress) {
                CurrentProject.Execute(this);
                CurrentProject = null;
            }
        }

        public void PerformExpansion() {
            TileBeingPursued = ExpansionLogic.GetNextTileToPursue(this);

            var costOfPursuit = ExpansionLogic.GetCultureCostOfAcquiringTile(this, TileBeingPursued);

            if(TileBeingPursued != null && costOfPursuit <= CultureStockpile && TilePossessionCanon.CanChangeOwnerOfTile(TileBeingPursued, this)) {

                CultureStockpile -= costOfPursuit;
                TilePossessionCanon.ChangeOwnerOfTile(TileBeingPursued, this);

                TileBeingPursued = ExpansionLogic.GetNextTileToPursue(this);
            }
        }

        public void PerformDistribution() {
            var availableSlots = GetAllAvailableSlots();
            DistributionLogic.DistributeWorkersIntoSlots(Population, availableSlots, this, DistributionPreferences);
        }

        public void PerformIncome(){
            LastIncome = ResourceGenerationLogic.GetTotalYieldForCity(this);

            CultureStockpile += LastIncome[ResourceType.Culture];
            FoodStockpile += LastIncome[ResourceType.Food] - GrowthLogic.GetFoodConsumptionPerTurn(this);
        }

        #endregion 

        private List<IWorkerSlot> GetAllAvailableSlots() {
            var retval = new List<IWorkerSlot>();

            retval.AddRange(TilePossessionCanon.GetTilesOfCity(this).Select(tile => tile.WorkerSlot));

            foreach(var building in BuildingPossessionCanon.GetBuildingsInCity(this)) {
                retval.AddRange(building.Slots);
            }

            return retval;
        }

        #endregion

    }

}
