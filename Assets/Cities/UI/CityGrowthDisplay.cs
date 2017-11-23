using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

namespace Assets.Cities.UI {

    public class CityGrowthDisplay : MonoBehaviour, ICityGrowthDisplay {

        #region instance fields and properties

        #region from ICityGrowthDisplay

        public ICity CityToDisplay { get; set; }

        #endregion

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

        #region from ICityGrowthDisplay

        public void Refresh() {
            if(CityToDisplay == null) {
                return;
            }

            DisplayGrowthInformation(CityToDisplay.Population, CityToDisplay.FoodStockpile, 
                GrowthLogic.GetFoodStockpileToGrow(CityToDisplay));
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
