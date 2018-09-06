using System.Collections.Generic;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IHomelandBalancer {

        #region methods

        void BalanceHomelandYields(HomelandData homelandData);

        #endregion

    }
}