using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Units;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// The standard implementation of ICity.
    /// </summary>
    /// <remarks>
    /// This class makes heavy use of the Humble Object pattern. Most of its logic involves
    /// calling into various logic- and canon-suffixed classes that handle the bulk of 
    /// behavior. Bugs are more likely to emerge from those classes rather than this one.
    /// </remarks>
    public class City : MonoBehaviour, ICity{

        #region instance fields and properties

        #region from ICity

        /// <inheritdoc/>
        public int Population {
            get { return _population; }
            set { _population = value; }
        }
        [SerializeField] private int _population;

        /// <inheritdoc/>
        public int FoodStockpile {
            get { return _foodStockpile; }
            set { _foodStockpile = value; }
        }
        [SerializeField] private int _foodStockpile;

        /// <inheritdoc/>
        public int CultureStockpile {
            get { return _cultureStockpile; }
            set { _cultureStockpile = value; }
        }
        [SerializeField] private int _cultureStockpile;

        /// <inheritdoc/>
        public ResourceSummary LastIncome { get; private set; }

        /// <inheritdoc/>
        public IProductionProject ActiveProject { get; private set; }

        /// <inheritdoc/>
        public ResourceFocusType ResourceFocus { get; set; }

        /// <inheritdoc/>
        public IHexCell CellBeingPursued { get; private set; }

        public IUnit CombatFacade { get; set; } 

        #endregion

        private IPopulationGrowthLogic GrowthLogic;

        private IProductionLogic ProductionLogic;

        private IResourceGenerationLogic ResourceGenerationLogic;

        private IBorderExpansionLogic ExpansionLogic;

        private IPossessionRelationship<ICity, IHexCell> TilePossessionCanon;

        private IWorkerDistributionLogic DistributionLogic;

        private CitySignals Signals;

        private IProductionProjectFactory ProjectFactory;

        private ICityConfig Config;

        #endregion

        #region instance methods

        /// <summary>
        /// Constructor-like method to facilitate dependency injection with the Zenject framework.
        /// </summary>
        /// <param name="growthLogic"></param>
        /// <param name="productionLogic"></param>
        /// <param name="resourceGenerationLogic"></param>
        /// <param name="expansionLogic"></param>
        /// <param name="tilePossessionCanon"></param>
        /// <param name="distributionLogic"></param>
        /// <param name="projectFactory"></param>
        /// <param name="signals"></param>
        [Inject]
        public void InjectDependencies(
            IPopulationGrowthLogic growthLogic, IProductionLogic productionLogic, 
            IResourceGenerationLogic resourceGenerationLogic, IBorderExpansionLogic expansionLogic,
            IPossessionRelationship<ICity, IHexCell> tilePossessionCanon, IWorkerDistributionLogic distributionLogic,
            IProductionProjectFactory projectFactory, CitySignals signals, ICityConfig config
        ){
            GrowthLogic             = growthLogic;
            ProductionLogic         = productionLogic;
            ResourceGenerationLogic = resourceGenerationLogic;
            ExpansionLogic          = expansionLogic;
            TilePossessionCanon     = tilePossessionCanon;
            DistributionLogic       = distributionLogic;
            ProjectFactory          = projectFactory;
            Signals                 = signals;
            Config                  = config;
        }

        #region Unity messages

        private void OnDestroy() {
            DestroyImmediate(CombatFacade.gameObject);
            Signals.CityBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #region from ICity

        /// <inheritdoc/>
        public void SetActiveProductionProject(IBuildingTemplate template) {
            if(template == null) {
                ActiveProject = null;
            }else {
                ActiveProject = ProjectFactory.ConstructBuildingProject(template);
            }
            Signals.ProjectChangedSignal.Fire(this, ActiveProject);
        }

        /// <inheritdoc/>
        public void SetActiveProductionProject(IUnitTemplate template) {
            if(template == null) {
                ActiveProject = null;
            }else {
                ActiveProject = ProjectFactory.ConstructUnitProject(template);
            }
            Signals.ProjectChangedSignal.Fire(this, ActiveProject);
        }

        /// <inheritdoc/>
        public void PerformGrowth() {
            if(FoodStockpile < 0 && Population > 1) {
                Population--;
                FoodStockpile = GrowthLogic.GetFoodStockpileAfterStarvation(this);

            }else if(FoodStockpile >= GrowthLogic.GetFoodStockpileToGrow(this)) {
                FoodStockpile = GrowthLogic.GetFoodStockpileAfterGrowth(this);
                Population++;
            }
        }

        /// <inheritdoc/>
        public void PerformProduction() {
            if(ActiveProject == null) {
                return;
            }

            ActiveProject.Progress += ProductionLogic.GetProductionProgressPerTurnOnProject(this, ActiveProject);

            if( ActiveProject.Progress >= ActiveProject.ProductionToComplete) {
                ActiveProject.Execute(this);
                ActiveProject = null;
            }
        }

        /// <inheritdoc/>
        public void PerformExpansion() {
            CellBeingPursued = ExpansionLogic.GetNextCellToPursue(this);

            var costOfPursuit = ExpansionLogic.GetCultureCostOfAcquiringCell(this, CellBeingPursued);

            if( CellBeingPursued != null &&
                costOfPursuit <= CultureStockpile &&
                TilePossessionCanon.CanChangeOwnerOfPossession(CellBeingPursued, this)
            ) {
                CultureStockpile -= costOfPursuit;
                TilePossessionCanon.ChangeOwnerOfPossession(CellBeingPursued, this);

                CellBeingPursued = ExpansionLogic.GetNextCellToPursue(this);
            }
        }

        /// <inheritdoc/>
        public void PerformDistribution() {
            var allSlots = DistributionLogic.GetSlotsAvailableToCity(this);
            var occupiedAndLockedSlots = allSlots.Where(slot => slot.IsOccupied && slot.IsLocked);

            int populationToAssign = Population - occupiedAndLockedSlots.Count();

            populationToAssign = Math.Max(0, populationToAssign);

            var slotsToAssign = allSlots.Where(slot => !slot.IsLocked);

            DistributionLogic.DistributeWorkersIntoSlots(populationToAssign, slotsToAssign, this, ResourceFocus);

            Signals.DistributionPerformedSignal.Fire(this);
        }

        /// <inheritdoc/>
        public void PerformIncome(){
            LastIncome = ResourceGenerationLogic.GetTotalYieldForCity(this);

            CultureStockpile += Mathf.FloorToInt(LastIncome[ResourceType.Culture]);

            int foodConsumption = GrowthLogic.GetFoodConsumptionPerTurn(this);
            if(foodConsumption <= LastIncome[ResourceType.Food]) {
                FoodStockpile += Mathf.FloorToInt(GrowthLogic.GetFoodStockpileAdditionFromIncome(
                    this, LastIncome[ResourceType.Food] - foodConsumption
                ));
            }else {
                FoodStockpile -= Mathf.CeilToInt(foodConsumption - LastIncome[ResourceType.Food]);
            }
        }

        public void PerformHealing() {
            CombatFacade.Hitpoints += Config.HitPointRegenPerRound;
            CombatFacade.CurrentMovement = 1;
        }

        #endregion

        #endregion

    }

}
