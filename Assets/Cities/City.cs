using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.GameMap;

namespace Assets.Cities {

    public class City : MonoBehaviour, ICity {

        #region instance fields and properties

        #region from ICity

        public IMapTile Location { get; set; }

        public int Population {
            get { return _population; }
            set { _population = value; }
        }
        [SerializeField] private int _population;

        public ReadOnlyCollection<IBuilding> Buildings {
            get { return buildings.AsReadOnly(); }
        }
        private List<IBuilding> buildings = new List<IBuilding>();

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

        private ITilePossessionCanon PossessionCanon;

        private IWorkerDistributionLogic DistributionLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPopulationGrowthLogic growthLogic, IProductionLogic productionLogic, 
            IResourceGenerationLogic resourceGenerationLogic, IBorderExpansionLogic expansionLogic,
            ITilePossessionCanon possessionCanon, IWorkerDistributionLogic distributionLogic
        ){
            GrowthLogic = growthLogic;
            ProductionLogic = productionLogic;
            ResourceGenerationLogic = resourceGenerationLogic;
            ExpansionLogic = expansionLogic;
            PossessionCanon = possessionCanon;
            DistributionLogic = distributionLogic;
        }

        #region from ICity

        public void AddBuilding(IBuilding building) {
            throw new NotImplementedException();
        }

        public void RemoveBuilding(IBuilding building) {
            throw new NotImplementedException();
        }

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
                CurrentProject.ExecuteProject(this);
                CurrentProject = null;
            }
        }

        public void PerformExpansion() {
            TileBeingPursued = ExpansionLogic.GetNextTileToPursue(this);

            var costOfPursuit = ExpansionLogic.GetCultureCostOfAcquiringTile(this, TileBeingPursued);

            if(TileBeingPursued != null && costOfPursuit <= CultureStockpile && PossessionCanon.CanChangeOwnerOfTile(TileBeingPursued, this)) {

                CultureStockpile -= costOfPursuit;
                PossessionCanon.ChangeOwnerOfTile(TileBeingPursued, this);

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

            retval.AddRange(PossessionCanon.GetTilesOfCity(this).Select(tile => tile.WorkerSlot));

            foreach(var building in Buildings) {
                retval.AddRange(building.WorkerSlots);
            }

            return retval;
        }

        #endregion

    }

}
