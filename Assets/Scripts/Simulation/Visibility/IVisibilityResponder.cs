using System;

namespace Assets.Simulation.Visibility {

    public interface IVisibilityResponder {

        #region properties

        bool UpdateVisibility { get; set; }

        #endregion

        #region methods

        void TryResetCellVisibility();
        void TryResetResourceVisibility();

        #endregion

    }

}