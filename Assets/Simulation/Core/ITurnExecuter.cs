using System;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Core {

    public interface ITurnExecuter {

        #region methods

        void BeginTurnOnCity(ICity city);
        void EndTurnOnCity(ICity city);

        #endregion

    }

}