using System;

namespace Assets.Simulation.Civilizations {

    public interface IVisibilityResponder {

        #region properties

        bool UpdateVisibility { get; set; }

        #endregion

        #region methods

        void TryResetAllVisibility();

        #endregion

    }

}