using System;

using Assets.Simulation.Players;

namespace Assets.UI {

    public interface ICameraFocuser {

        #region methods

        void ReturnFocusToPlayer(IPlayer player);

        #endregion

    }

}