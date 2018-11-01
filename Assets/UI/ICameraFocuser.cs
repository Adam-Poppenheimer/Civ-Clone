using System;

namespace Assets.UI {

    public interface ICameraFocuser {

        #region methods

        void ActivateBeginTurnFocusing();
        void DeactivateBeginTurnFocusing();

        #endregion

    }

}