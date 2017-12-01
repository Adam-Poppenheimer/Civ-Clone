using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Growth;

namespace Assets.UI.Cities.Growth {

    public class CityGrowthDisplay : CityDisplayBase {

        #region instance fields and properties

        [InjectOptional(Id = "Current Population Field")]
        public Text CurrentPopulationField {
            get { return _currentPopulationField; }
            set {
                if(value != null) {
                    _currentPopulationField = value;
                }
            }
        }
        [SerializeField] private Text _currentPopulationField;

        [InjectOptional(Id = "Current Food Stockpile Field")]
        public Text CurrentFoodStockpileField {
            get { return _currentFoodStockpileField; }
            set {
                if(value != null) {
                    _currentFoodStockpileField = value;
                }
            }
        }
        [SerializeField] private Text _currentFoodStockpileField;

        [InjectOptional(Id = "Food Until Next Growth Field")]
        public Text FoodUntilNextGrowthField {
            get { return _foodUntilNextGrowthField; }
            set {
                if(value != null) {
                    _foodUntilNextGrowthField = value;
                }
            }
        }
        [SerializeField] private Text _foodUntilNextGrowthField;

        [InjectOptional(Id = "Change Status Field")]
        public Text ChangeStatusField {
            get { return _changeStatusField; }
            set {
                if(value != null) {
                    _changeStatusField = value;
                }
            }
        }
        [SerializeField] private Text _changeStatusField;

        [InjectOptional(Id = "Growth Slider")]
        public Slider GrowthSlider {
            get { return _growthSlider; }
            set {
                if(value != null) {
                    _growthSlider = value;
                }
            }
        }
        [SerializeField] private Slider _growthSlider;

        private IPopulationGrowthLogic GrowthLogic;

        private IResourceGenerationLogic GenerationLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IPopulationGrowthLogic growthLogic, IResourceGenerationLogic generationLogic) {
            GrowthLogic = growthLogic;
            GenerationLogic = generationLogic;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(CityToDisplay == null) {
                return;
            }

            DisplayGrowthInformation(CityToDisplay.Population, CityToDisplay.FoodStockpile, GrowthLogic.GetFoodStockpileToGrow(CityToDisplay));
        }

        #endregion

        private void DisplayGrowthInformation(int currentPopulation, int currentFoodStockpile,
            int foodUntilNextGrowth) {

            CurrentPopulationField.text = currentPopulation.ToString();
            CurrentFoodStockpileField.text = currentFoodStockpile.ToString();
            FoodUntilNextGrowthField.text = foodUntilNextGrowth.ToString();

            GrowthSlider.minValue = 0;
            GrowthSlider.maxValue = foodUntilNextGrowth;
            GrowthSlider.value = currentFoodStockpile;

            var netIncome = 
                GenerationLogic.GetTotalYieldForCity(CityToDisplay)[ResourceType.Food] -
                GrowthLogic.GetFoodConsumptionPerTurn(CityToDisplay);

            if(netIncome > 0) {
                int turnsUntilGrowth = Mathf.CeilToInt((foodUntilNextGrowth - currentFoodStockpile) / (float)netIncome);

                ChangeStatusField.text = string.Format("{0} turns until growth", turnsUntilGrowth);
            }else if(netIncome == 0) {
                ChangeStatusField.text = "Stagnation";
            }else {
                int turnsUntilStarvation = Mathf.CeilToInt(currentFoodStockpile / -(float)netIncome);
                ChangeStatusField.text = string.Format("{0} turns until starvation", turnsUntilStarvation);
            }
        }

        #endregion

    }

}
