using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Simulation.Core {

    /// <summary>
    /// The standard implementation of ITurnExecuter.
    /// </summary>
    public class TurnExecuter : ITurnExecuter {

        #region instance methods

        #region from ITurnExecuter

        /// <inheritdoc/>
        public void BeginTurnOnCity(ICity city) {
            city.PerformProduction();
            city.PerformGrowth();
            city.PerformExpansion();
            city.PerformDistribution();
        }

        /// <inheritdoc/>
        public void EndTurnOnCity(ICity city) {
            city.PerformIncome();
        }

        /// <inheritdoc/>
        public void BeginTurnOnCivilization(ICivilization civilization) {
            civilization.PerformIncome();
        }

        /// <inheritdoc/>
        public void EndTurnOnCivilization(ICivilization civilization) {
            
        }

        /// <inheritdoc/>
        public void BeginTurnOnUnit(IUnit unit) {
            unit.CurrentMovement = unit.Template.MaxMovement;
        }

        /// <inheritdoc/>
        public void EndTurnOnUnit(IUnit unit) {
            unit.PerformMovement();
        }

        #endregion

        #endregion

    }

}
