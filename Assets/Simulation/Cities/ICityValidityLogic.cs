using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    public interface ICityValidityLogic {

        #region methods

        bool IsTileValidForCity(IHexCell tile);

        #endregion

    }

}