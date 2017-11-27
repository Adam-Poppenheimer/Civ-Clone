using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Growth;

namespace Assets.UI.Cities.Growth {

    public class CityGrowthDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text CurrentPopulationField;
        [SerializeField] private Text CurrentFoodStockpileField;
        [SerializeField] private Text FoodUntilNextGrowthField;

        [SerializeField] private Slider GrowthSlider;

        private IPopulationGrowthLogic GrowthLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IPopulationGrowthLogic growthLogic) {
            GrowthLogic = growthLogic;
        }

        #region from CityDisplayBase

        protected override void DisplayCity(ICity city) {
            DisplayGrowthInformation(city.Population, city.FoodStockpile, GrowthLogic.GetFoodStockpileToGrow(city));
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
        }

        #endregion

    }

}
