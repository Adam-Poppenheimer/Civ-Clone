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
    public class RoundExecuter : IRoundExecuter {

        #region instance methods

        #region from ITurnExecuter

        /// <inheritdoc/>
        public void BeginRoundOnCity(ICity city) {
            city.PerformProduction();
            city.PerformGrowth();
            city.PerformExpansion();
            city.PerformDistribution();
            city.PerformHealing();
        }

        /// <inheritdoc/>
        public void EndRoundOnCity(ICity city) {
            city.PerformIncome();
        }

        /// <inheritdoc/>
        public void BeginRoundOnCivilization(ICivilization civilization) {
            civilization.PerformIncome();
            civilization.PerformResearch();
        }

        /// <inheritdoc/>
        public void EndRoundOnCivilization(ICivilization civilization) {
            
        }

        /// <inheritdoc/>
        public void BeginRoundOnUnit(IUnit unit) {
            unit.CurrentMovement = unit.MaxMovement;
        }

        /// <inheritdoc/>
        public void EndRoundOnUnit(IUnit unit) {
            unit.PerformMovement();
            unit.HasAttacked = false;
        }

        #endregion

        #endregion

    }

}
