using System;

namespace Assets.Simulation.Civilizations {

    public interface ICivDefeatExecutor {

        #region properties

        bool CheckForDefeat { get; set; }

        #endregion

        #region methods

        void PerformDefeatOfCiv(ICivilization civ);

        bool ShouldCivBeDefeated(ICivilization civ);

        #endregion

    }

}