using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Cities {

    public class CityHappinessLogic : ICityHappinessLogic {

        #region instance fields and properties

        private ICityConfig Config;

        #endregion

        #region constructors
        
        [Inject]
        public CityHappinessLogic(ICityConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ICityHappinessLogic

        public int GetLocalHappinessOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }
            
            return 0;
        }

        public int GetGlobalHappinessOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return 0;
        }

        public int GetTotalHappinessofCity(ICity city) {
            return GetLocalHappinessOfCity(city) + GetGlobalHappinessOfCity(city);
        }

        public int GetUnhappinessOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            int retval = Config.UnhappinessPerCity;

            retval += Mathf.RoundToInt(Config.UnhappinessPerPopulation * city.Population);

            return retval;
        }

        public int GetNetHappinessOfCity(ICity city) {
            return GetTotalHappinessofCity(city) - GetUnhappinessOfCity(city);
        }

        #endregion

        #endregion

    }

}
