using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.GameMap {

    public interface ITileResourceLogic {

        #region methods

        ResourceSummary GetYieldOfTile(IMapTile tile);

        #endregion

    }

}
