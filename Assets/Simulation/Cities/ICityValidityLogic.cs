using System;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Cities {

    public interface ICityValidityLogic {

        #region methods

        bool IsTileValidForCity(IMapTile tile);

        #endregion

    }

}