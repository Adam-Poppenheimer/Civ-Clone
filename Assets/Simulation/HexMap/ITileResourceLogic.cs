using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface ITileResourceLogic {

        #region methods

        ResourceSummary GetYieldOfTile(IHexCell tile);

        #endregion

    }

}
