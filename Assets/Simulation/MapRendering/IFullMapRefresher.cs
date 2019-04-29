using System;

namespace Assets.Simulation.MapRendering {

    public interface IFullMapRefresher {

        #region properties

        bool IsRefreshingFarmland { get; }
        bool IsRefreshingRivers   { get; }

        #endregion

        #region methods

        void RefreshFarmland();
        void RefreshRivers();

        #endregion

    }

}