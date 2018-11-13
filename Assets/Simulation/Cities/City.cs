using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

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
    public class City : MonoBehaviour, ICity {

        #region instance fields and properties

        #region from ICity

        public string Name { get; set; }

        public Vector3 Position {
            get { return transform.position; }
        }

        /// <inheritdoc/>
        public int Population {
            get { return _population; }
            set { _population = value; }
        }
        [SerializeField] private int _population;

        /// <inheritdoc/>
        public float FoodStockpile {
            get { return _foodStockpile; }
            set { _foodStockpile = value; }
        }
        [SerializeField] private float _foodStockpile;

        /// <inheritdoc/>
        public int CultureStockpile {
            get { return _cultureStockpile; }
            set { _cultureStockpile = value; }
        }
        [SerializeField] private int _cultureStockpile;

        /// <inheritdoc/>
        public YieldSummary LastIncome { get; private set; }

        /// <inheritdoc/>
        public IProductionProject ActiveProject {
            get { return _activeProject; }
            set {
                if(_activeProject != value) {
                    _activeProject = value;
                    Signals.ProjectChangedSignal.OnNext(new UniRx.Tuple<ICity, IProductionProject>(this, _activeProject));
                }
            }
        }
        private IProductionProject _activeProject;

        /// <inheritdoc/>
        public YieldFocusType YieldFocus { get; set; }

        /// <inheritdoc/>
        public IHexCell CellBeingPursued { get; private set; }

        public IUnit CombatFacade { get; set; } 

        #endregion

        private IPopulationGrowthLogic                   GrowthLogic;
        private IProductionLogic                         ProductionLogic;
        private IYieldGenerationLogic                    YieldGenerationLogic;
        private IBorderExpansionLogic                    ExpansionLogic;
        private IPossessionRelationship<ICity, IHexCell> TilePossessionCanon;
        private IWorkerDistributionLogic                 DistributionLogic;
        private ICityProductionResolver                  ProductionResolver;
        private CitySignals                              Signals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPopulationGrowthLogic growthLogic, IProductionLogic productionLogic, 
            IYieldGenerationLogic resourceGenerationLogic, IBorderExpansionLogic expansionLogic,
            IPossessionRelationship<ICity, IHexCell> tilePossessionCanon,
            IWorkerDistributionLogic distributionLogic, ICityProductionResolver cityProductionResolver,
            CitySignals signals
        ){
            GrowthLogic          = growthLogic;
            ProductionLogic      = productionLogic;
            YieldGenerationLogic = resourceGenerationLogic;
            ExpansionLogic       = expansionLogic;
            TilePossessionCanon  = tilePossessionCanon;
            DistributionLogic    = distributionLogic;
            ProductionResolver   = cityProductionResolver;
            Signals              = signals;
        }

        #region Unity messages

        private void OnDestroy() {
            Signals.CityBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #region from ICity

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
                ProductionResolver.MakeProductionRequest(ActiveProject, this);
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

            DistributionLogic.DistributeWorkersIntoSlots(populationToAssign, slotsToAssign, this, YieldFocus);

            Signals.DistributionPerformedSignal.OnNext(this);
        }

        /// <inheritdoc/>
        public void PerformIncome(){
            LastIncome = YieldGenerationLogic.GetTotalYieldForCity(this);

            CultureStockpile += Mathf.FloorToInt(LastIncome[YieldType.Culture]);

            int foodConsumption = GrowthLogic.GetFoodConsumptionPerTurn(this);
            if(foodConsumption <= LastIncome[YieldType.Food]) {
                FoodStockpile += GrowthLogic.GetFoodStockpileAdditionFromIncome(
                    this, LastIncome[YieldType.Food] - foodConsumption
                );
            }else {
                FoodStockpile -= foodConsumption - LastIncome[YieldType.Food];
            }
        }

        public void Destroy() {
            if(Application.isPlaying) {
                Destroy(gameObject);
            }else {
                DestroyImmediate(gameObject);
            }
        }

        #endregion

        #endregion

    }

}
