using System;

namespace Assets.Simulation.Civilizations {

    public interface ICapitalCitySynchronizer {

        #region properties

        bool IsUpdatingCapitals { get; }

        #endregion

        #region methods

        void SetCapitalUpdating(bool updateCapitals);

        #endregion

    }

}