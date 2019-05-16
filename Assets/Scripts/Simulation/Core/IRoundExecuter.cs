using System;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Core {

    public interface IRoundExecuter {

        #region methods

        void PerformStartOfRoundActions();
        void PerformEndOfRoundActions();

        #endregion

    }

}