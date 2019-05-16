using System.Collections.Generic;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IBakedElementsLogic {

        #region methods

        BakedElementFlags GetBakedElementsInCells(IEnumerable<IHexCell> cells);

        #endregion

    }

}