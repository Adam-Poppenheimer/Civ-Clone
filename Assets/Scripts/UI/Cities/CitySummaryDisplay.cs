﻿using System;
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

        [SerializeField] private Button RangedAttackButton = null;

        [SerializeField] private Text PopulationField                   = null;
        [SerializeField] private Text NameField                         = null;
        [SerializeField] private Text TurnsUntilGrowthField             = null;
        [SerializeField] private Text TurnsUntilProductionFinishedField = null;

        [SerializeField] private Slider GrowthSlider     = null;
        [SerializeField] private Slider ProductionSlider = null;

        [SerializeField] private Slider HealthSlider = null;

        public RectTransform RectTransform {
            get {
                if(_rectTransform == null) {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }
        private RectTransform _rectTransform;



        private IHexGrid                                      Grid;        
        private ICityConfig                                   Config;
        private IUnitPositionCanon                            UnitPositionCanon;
        private ICombatExecuter                               CombatExecuter;
        private UIStateMachineBrain                           Brain;
        private IPopulationGrowthLogic                        GrowthLogic;
        private IProductionLogic                              ProductionLogic;
        private IYieldGenerationLogic                         YieldGenerationLogic;
        private IGameCore                                     GameCore;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private Animator                                      UIAnimator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGrid grid, ICityConfig config, IUnitPositionCanon unitPositionCanon,
            ICombatExecuter combatExecuter, UIStateMachineBrain brain,
            IPopulationGrowthLogic growthLogic, IProductionLogic productionLogic,
            IYieldGenerationLogic resourceGenerationLogic, IGameCore gameCore,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            [Inject(Id = "UI Animator")] Animator uiAnimator
        ) {
            Grid                 = grid;
            Config               = config;
            UnitPositionCanon    = unitPositionCanon;
            CombatExecuter       = combatExecuter;
            Brain                = brain;
            GrowthLogic          = growthLogic;
            ProductionLogic      = productionLogic;
            YieldGenerationLogic = resourceGenerationLogic;
            GameCore             = gameCore;
            CityPossessionCanon  = cityPossessionCanon;
            CityLocationCanon    = cityLocationCanon;
            UIAnimator           = uiAnimator;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            DisplayData();

            RangedAttackButton.gameObject.SetActive(HasRangedAttackTarget() && IsCityOwnedByActiveCiv());
        }

        #endregion

        public void OnRangedAttackRequested() {
            Brain.LastUnitClicked = ObjectToDisplay.CombatFacade;
            UIAnimator.SetTrigger("City Ranged Attack Requested");
        }

        private bool HasRangedAttackTarget() {
            var cityLocation = CityLocationCanon.GetOwnerOfPossession(ObjectToDisplay);

            foreach(var cell in Grid.GetCellsInRadius(cityLocation, Config.CityAttackRange)) {
                foreach(var unit in UnitPositionCanon.GetPossessionsOfOwner(cell)) {
                    if(CombatExecuter.CanPerformRangedAttack(ObjectToDisplay.CombatFacade, unit)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsCityOwnedByActiveCiv() {
            return CityPossessionCanon.GetOwnerOfPossession(ObjectToDisplay) == GameCore.ActiveCiv;
        }

        private void DisplayData() {
            NameField.text = ObjectToDisplay.Name;

            DisplayGrowthData();
            
            if(ObjectToDisplay.ActiveProject != null) {
                DisplayProductionData();
            }else {
                ClearProductionData();
            }

            if(ObjectToDisplay.CombatFacade.CurrentHitpoints < ObjectToDisplay.CombatFacade.MaxHitpoints) {
                DisplayHealth();
            }else {
                ClearHealth();
            }
        }

        private void DisplayGrowthData() {
            YieldSummary income = YieldGenerationLogic.GetTotalYieldForCity(ObjectToDisplay);

            float currentFoodStockpile = ObjectToDisplay.FoodStockpile;
            int foodUntilNextGrowth = GrowthLogic.GetFoodStockpileToGrow(ObjectToDisplay);
            float netFoodIncome = income[YieldType.Food] - GrowthLogic.GetFoodConsumptionPerTurn(ObjectToDisplay);

            float realFoodGain = GrowthLogic.GetFoodStockpileAdditionFromIncome(ObjectToDisplay, netFoodIncome);

            int turnsToGrow = Mathf.CeilToInt((foodUntilNextGrowth - currentFoodStockpile) / realFoodGain);

            if(realFoodGain > 0) {
                TurnsUntilGrowthField.text = turnsToGrow.ToString();
            }else {
                TurnsUntilGrowthField.text = "--";
            }
            
            TurnsUntilGrowthField.text = turnsToGrow.ToString();

            PopulationField.text = ObjectToDisplay.Population.ToString();

            GrowthSlider.minValue = 0;
            GrowthSlider.maxValue = foodUntilNextGrowth;
            GrowthSlider.value    = Mathf.FloorToInt(currentFoodStockpile);
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
            if(HealthSlider == null) {
                return;
            }

            HealthSlider.gameObject.SetActive(true);

            HealthSlider.minValue = 0;
            HealthSlider.maxValue = ObjectToDisplay.CombatFacade.MaxHitpoints;
            HealthSlider.value    = ObjectToDisplay.CombatFacade.CurrentHitpoints;
        }

        private void ClearHealth() {
            if(HealthSlider == null) {
                return;
            }

            HealthSlider.gameObject.SetActive(false);
        }

        #endregion

    }

}
