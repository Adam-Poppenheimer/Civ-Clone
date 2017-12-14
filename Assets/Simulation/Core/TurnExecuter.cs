using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

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

        public void BeginTurnOnUnit(IUnit unit) {
            unit.CurrentMovement = unit.Template.MaxMovement;
        }

        public void EndTurnOnUnit(IUnit unit) {
            unit.PerformMovement();
        }

        #endregion

        #endregion

    }

}
