using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IRiverContourRationalizer {

        #region methods

        void RationalizeRiverContoursInCorner(IHexCell center, HexDirection direction);

        #endregion

    }

}