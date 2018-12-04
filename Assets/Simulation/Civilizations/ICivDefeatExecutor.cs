using System;

using Assets.Simulation.Core;

namespace Assets.Simulation.Civilizations {

    public interface ICivDefeatExecutor : IPlayModeSensitiveElement {

        #region methods

        void PerformDefeatOfCiv(ICivilization civ);

        bool ShouldCivBeDefeated(ICivilization civ);

        #endregion

    }

}