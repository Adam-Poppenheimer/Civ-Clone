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
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Growth;

namespace Assets.UI.Cities.Growth {

    public class CityGrowthDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text       CurrentPopulationTextField  = null;
        [SerializeField] private InputField CurrentPopulationInputField = null;

        [SerializeField] private Text       CurrentFoodStockpileTextField  = null;
        [SerializeField] private InputField CurrentFoodStockpileInputField = null;

        [SerializeField] private Text NetGainField  = null;

        [SerializeField] private Text FoodUntilNextGrowthField = null;
        [SerializeField] private Text ChangeStatusField = null;

        [SerializeField] private Slider GrowthSlider = null;

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();





        private IPopulationGrowthLogic GrowthLogic;
        private IYieldGenerationLogic  GenerationLogic;
        private CitySignals            CitySignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPopulationGrowthLogic growthLogic, IYieldGenerationLogic generationLogic,
            CitySignals citySignals
        ) {
            GrowthLogic     = growthLogic;
            GenerationLogic = generationLogic;
            CitySignals     = citySignals;
        }

        #region Unity messages

        private void Awake() {
            CurrentPopulationInputField   .onEndEdit.AddListener(OnPopulationInputFieldChanged);
            CurrentFoodStockpileInputField.onEndEdit.AddListener(OnFoodStockpileInputFieldChanged);
        }

        #endregion

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            DisplayGrowthInformation(
                ObjectToDisplay.Population, ObjectToDisplay.FoodStockpile,
                GrowthLogic.GetFoodStockpileToGrow(ObjectToDisplay)
            );
        }

        protected override void DoOnEnable() {
            SignalSubscriptions.Add(CitySignals.PopulationChanged    .Subscribe(city => Refresh()));
            SignalSubscriptions.Add(CitySignals.FoodStockpileChanged .Subscribe(city => Refresh()));
            SignalSubscriptions.Add(CitySignals.GainedBuilding   .Subscribe(data => Refresh()));
            SignalSubscriptions.Add(CitySignals.LostBuilding     .Subscribe(data => Refresh()));
            SignalSubscriptions.Add(CitySignals.DistributionPerformed.Subscribe(city => Refresh()));
        }

        protected override void DoOnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
            SignalSubscriptions.Clear();
        }

        #endregion

        private void DisplayGrowthInformation(
            int currentPopulation, float currentFoodStockpile, int foodUntilNextGrowth
        ) {
            if(DisplayType == CityDisplayType.PlayMode) {
                CurrentPopulationTextField   .gameObject.SetActive(true);
                CurrentFoodStockpileTextField.gameObject.SetActive(true);

                CurrentPopulationInputField  .gameObject.SetActive(false);
                CurrentFoodStockpileInputField.gameObject.SetActive(false);

                CurrentPopulationTextField   .text = currentPopulation.ToString();
                CurrentFoodStockpileTextField.text = Mathf.RoundToInt(currentFoodStockpile).ToString();

            }else if(DisplayType == CityDisplayType.MapEditor) {
                CurrentPopulationTextField   .gameObject.SetActive(false);
                CurrentFoodStockpileTextField.gameObject.SetActive(false);

                CurrentPopulationInputField   .gameObject.SetActive(true);
                CurrentFoodStockpileInputField.gameObject.SetActive(true);

                CurrentPopulationInputField   .text = currentPopulation.ToString();
                CurrentFoodStockpileInputField.text = Mathf.RoundToInt(currentFoodStockpile).ToString();
            }
            
            FoodUntilNextGrowthField.text = foodUntilNextGrowth.ToString();

            GrowthSlider.minValue = 0;
            GrowthSlider.maxValue = foodUntilNextGrowth;
            GrowthSlider.value = Mathf.RoundToInt(currentFoodStockpile);

            float netIncome = 
                GenerationLogic.GetTotalYieldForCity(ObjectToDisplay)[YieldType.Food] -
                GrowthLogic.GetFoodConsumptionPerTurn(ObjectToDisplay);

            float foodGain = GrowthLogic.GetFoodStockpileAdditionFromIncome(ObjectToDisplay, netIncome);

            

            if(netIncome > 0) {
                int turnsUntilGrowth = Mathf.CeilToInt((foodUntilNextGrowth - currentFoodStockpile) / (float)foodGain);
                ChangeStatusField.text = string.Format("{0} turns until growth", turnsUntilGrowth);
                NetGainField.text = string.Format("(+{0})", foodGain);

            }else if(netIncome == 0) {
                ChangeStatusField.text = "Stagnation";                
                NetGainField.text = "(-)";

            }else {
                int turnsUntilStarvation = Mathf.CeilToInt(currentFoodStockpile / -foodGain);
                ChangeStatusField.text = string.Format("{0} turns until starvation", turnsUntilStarvation);
                NetGainField.text = string.Format("({0})", netIncome);
            }
        }

        private void OnPopulationInputFieldChanged(string text) {
            int newPopulation;

            if(int.TryParse(text, out newPopulation)) {
                ObjectToDisplay.Population = newPopulation;
            }
        }

        private void OnFoodStockpileInputFieldChanged(string text) {
            float newFood;

            if(float.TryParse(text, out newFood)) {
                ObjectToDisplay.FoodStockpile = newFood;
            }
        }

        #endregion

    }

}
