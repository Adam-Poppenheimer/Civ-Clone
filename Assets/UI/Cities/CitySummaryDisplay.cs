using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;

using Assets.UI.StateMachine;

namespace Assets.UI.Cities {

    public class CitySummaryDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Button RangedAttackButton;

        [SerializeField] private Text PopulationField;
        [SerializeField] private Text NameField;
        [SerializeField] private Text TurnsUntilGrowthField;
        [SerializeField] private Text TurnsUntilProductionFinishedField;

        [SerializeField] private Slider GrowthSlider;
        [SerializeField] private Slider ProductionSlider;

        [SerializeField] private Slider HealthSlider;



        private IHexGrid Grid;
        
        private ICityConfig Config;

        private IUnitPositionCanon UnitPositionCanon;

        private ICombatExecuter CombatExecuter;

        private UIStateMachineBrain Brain;

        private IPopulationGrowthLogic GrowthLogic;

        private IProductionLogic ProductionLogic;

        private IResourceGenerationLogic ResourceGenerationLogic;

        private IGameCore GameCore;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private Animator UIAnimator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGrid grid, ICityConfig config, IUnitPositionCanon unitPositionCanon,
            ICombatExecuter combatExecuter, UIStateMachineBrain brain,
            IPopulationGrowthLogic growthLogic, IProductionLogic productionLogic,
            IResourceGenerationLogic resourceGenerationLogic, IGameCore gameCore,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            [Inject(Id = "UI Animator")] Animator uiAnimator
        ) {
            Grid                    = grid;
            Config                  = config;
            UnitPositionCanon       = unitPositionCanon;
            CombatExecuter          = combatExecuter;
            Brain                   = brain;
            GrowthLogic             = growthLogic;
            ProductionLogic         = productionLogic;
            ResourceGenerationLogic = resourceGenerationLogic;
            GameCore                = gameCore;
            CityPossessionCanon     = cityPossessionCanon;
            UIAnimator              = uiAnimator;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            DisplayData();

            RangedAttackButton.gameObject.SetActive(HasRangedAttackTarget() && IsCityOwnedByActiveCiv());

            transform.position = Camera.main.WorldToScreenPoint(ObjectToDisplay.transform.position);
        }

        #endregion

        public void OnRangedAttackRequested() {
            Brain.LastUnitClicked = ObjectToDisplay.CombatFacade;
            UIAnimator.SetTrigger("City Ranged Attack Requested");
        }

        private bool HasRangedAttackTarget() {
            foreach(var cell in Grid.GetCellsInRadius(ObjectToDisplay.Location, Config.CityAttackRange)) {
                foreach(var unit in UnitPositionCanon.GetPossessionsOfOwner(cell)) {
                    if(CombatExecuter.CanPerformRangedAttack(ObjectToDisplay.CombatFacade, unit)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsCityOwnedByActiveCiv() {
            return CityPossessionCanon.GetOwnerOfPossession(ObjectToDisplay) == GameCore.ActiveCivilization;
        }

        private void DisplayData() {   
            DisplayGrowthData();
            
            if(ObjectToDisplay.ActiveProject != null) {
                DisplayProductionData();
            }else {
                ClearProductionData();
            }

            if(ObjectToDisplay.CombatFacade.Health < ObjectToDisplay.CombatFacade.MaxHealth) {
                DisplayHealth();
            }else {
                ClearHealth();
            }
        }

        private void DisplayGrowthData() {
            ResourceSummary income = ResourceGenerationLogic.GetTotalYieldForCity(ObjectToDisplay);

            int currentFoodStockpile = ObjectToDisplay.FoodStockpile;
            int foodUntilNextGrowth = GrowthLogic.GetFoodStockpileToGrow(ObjectToDisplay);
            float netFoodIncome = income[ResourceType.Food] - GrowthLogic.GetFoodConsumptionPerTurn(ObjectToDisplay);

            int turnsToGrow = Mathf.CeilToInt((foodUntilNextGrowth - currentFoodStockpile) / netFoodIncome);

            if(netFoodIncome > 0) {
                TurnsUntilGrowthField.text = turnsToGrow.ToString();
            }else {
                TurnsUntilGrowthField.text = "--";
            }
            
            TurnsUntilGrowthField.text = turnsToGrow.ToString();

            PopulationField.text = ObjectToDisplay.Population.ToString();

            GrowthSlider.minValue = 0;
            GrowthSlider.maxValue = foodUntilNextGrowth;
            GrowthSlider.value    = currentFoodStockpile;
        }

        private void DisplayProductionData() {
            int currentProgress  = ObjectToDisplay.ActiveProject.Progress;
            int productionNeeded = ObjectToDisplay.ActiveProject.ProductionToComplete;

            int turnsToProduce = Mathf.CeilToInt(
                (float)(productionNeeded - currentProgress) /
                ProductionLogic.GetProductionProgressPerTurnOnProject(ObjectToDisplay, ObjectToDisplay.ActiveProject)
            );

            TurnsUntilProductionFinishedField.text = turnsToProduce.ToString();

            ProductionSlider.minValue = 0;
            ProductionSlider.maxValue = productionNeeded;
            ProductionSlider.value    = currentProgress;
        }

        private void ClearProductionData() {
            TurnsUntilProductionFinishedField.text = "--";

            ProductionSlider.minValue = 0;
            ProductionSlider.maxValue = 0;
            ProductionSlider.value    = 0;
        }

        private void DisplayHealth() {
            HealthSlider.gameObject.SetActive(true);

            HealthSlider.minValue = 0;
            HealthSlider.maxValue = ObjectToDisplay.CombatFacade.MaxHealth;
            HealthSlider.value    = ObjectToDisplay.CombatFacade.Health;
        }

        private void ClearHealth() {
            HealthSlider.gameObject.SetActive(false);
        }

        #endregion

    }

}
