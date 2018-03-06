using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Growth {

    /// <summary>
    /// The standard implementation for IPopulationGrowthLogic.
    /// </summary>
    public class PopulationGrowthLogic : IPopulationGrowthLogic {

        #region instance fields and properties

        private ICityConfig Config;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private ICivilizationHappinessLogic CivilizationHappinessLogic;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        [Inject]
        public PopulationGrowthLogic(ICityConfig config,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICivilizationHappinessLogic civilizationHappinessLogic
        ){
            Config                     = config;
            CityPossessionCanon        = cityPossessionCanon;
            CivilizationHappinessLogic = civilizationHappinessLogic;
        }

        #endregion

        #region instance methods

        #region from IPopulationGrowthLogic

        /// <inheritdoc/>
        public int GetFoodConsumptionPerTurn(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return city.Population * Config.FoodConsumptionPerPerson;
        }

        /// <inheritdoc/>
        public int GetFoodStockpileSubtractionAfterGrowth(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return GetFoodStockpileToGrow(city);
        }

        /// <inheritdoc/>
        public int GetFoodStockpileAfterStarvation(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return 0;
        }

        /// <inheritdoc/>
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

        public float GetFoodStockpileAdditionFromIncome(ICity city, float foodIncome) {
            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            var ownerHappiness = CivilizationHappinessLogic.GetNetHappinessOfCiv(cityOwner);

            if(ownerHappiness < 0) {
                if(ownerHappiness > -10) {
                    return foodIncome / 4f;
                }else {
                    return 0;
                }
            }else {
                return foodIncome;
            }
        }

        #endregion

        #endregion
        
    }

}
