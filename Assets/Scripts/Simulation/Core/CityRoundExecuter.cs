using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Core {

    public class CityRoundExecuter : IRoundExecuter {

        #region instance fields and properties

        private ICityFactory CityFactory;

        #endregion

        #region constructors

        [Inject]
        public CityRoundExecuter(ICityFactory cityFactory) {
            CityFactory = cityFactory;
        }

        #endregion

        #region instance methods

        #region from IRoundExecuter

        public void PerformStartOfRoundActions() {
            foreach(var city in CityFactory.AllCities) {
                city.PerformProduction();
                city.PerformGrowth();
                city.PerformExpansion();
                city.PerformDistribution();
            }
        }

        public void PerformEndOfRoundActions() {
            foreach(var city in CityFactory.AllCities) {
                city.PerformIncome();
            }
        }

        #endregion

        #endregion

    }

}
