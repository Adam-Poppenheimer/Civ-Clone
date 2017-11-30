using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

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

        public void BeginTurnOnCivilization(ICivilization civilization) {
            civilization.PerformIncome();
        }

        public void EndTurnOnCivilization(ICivilization civilization) {
            
        }

        #endregion

        #endregion

    }

}
