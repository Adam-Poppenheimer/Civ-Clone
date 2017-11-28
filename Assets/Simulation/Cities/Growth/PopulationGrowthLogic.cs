using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Cities.Growth {

    public class PopulationGrowthLogic : IPopulationGrowthLogic {

        #region instance fields and properties

        private ICityConfig Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityConfig config) {
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
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return GetFoodStockpileToGrow(city);
        }

        public int GetFoodStockpileAfterStarvation(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return 0;
        }

        public int GetFoodStockpileToGrow(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            int previousPopulation = city.Population -1;

            return Mathf.FloorToInt(
                Config.BaseGrowthStockpile + 
                Config.GrowthPreviousPopulationCoefficient * previousPopulation +
                Mathf.Pow(previousPopulation, Config.GrowthPreviousPopulationExponent)
            );
        }

        #endregion

        #endregion
        
    }

}
