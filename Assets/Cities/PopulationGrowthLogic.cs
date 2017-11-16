using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Cities {

    public class PopulationGrowthLogic : IPopulationGrowthLogic {

        #region instance fields and properties

        private IPopulationGrowthConfig Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IPopulationGrowthConfig config) {
            Config = config;
        }

        #region from IPopulationGrowthLogic

        public int GetFoodConsumptionPerTurn(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return city.Population * Config.FoodConsumptionPerPerson;
        }

        public int GetFoodStockpileSubtractionAfterGrowth(ICity city) {
            return GetFoodStockpileToGrow(city);
        }

        public int GetFoodStockpileAfterStarvation(ICity city) {
            return 0;
        }

        public int GetFoodStockpileToGrow(ICity city) {
            int previousPopulation = city.Population -1;

            return Mathf.FloorToInt(
                Config.BaseStockpile + 
                Config.PreviousPopulationCoefficient * previousPopulation +
                Mathf.Pow(previousPopulation, Config.PreviousPopulationExponent)
            );
        }

        #endregion

        #endregion
        
    }

}
