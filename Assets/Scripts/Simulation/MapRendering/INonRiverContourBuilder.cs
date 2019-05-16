using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface INonRiverContourBuilder {

        #region methods

        void BuildNonRiverContour(IHexCell center, HexDirection direction);

        #endregion

    }

}