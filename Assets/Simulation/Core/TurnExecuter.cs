using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Core {

    public class TurnExecuter : ITurnExecuter {

        #region instance methods

        #region from ITurnExecuter

        public void BeginTurnOnCity(ICity city) {
            city.PerformProduction();
            city.PerformGrowth();
            city.PerformExpansion();
            city.PerformDistribution();
        }

        public void EndTurnOnCity(ICity city) {
            city.PerformIncome();
        }

        #endregion

        #endregion

    }

}
