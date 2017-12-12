using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units {

    public interface IUnitTerrainCostLogic {

        #region methods

        int GetCostToMoveUnitIntoTile(IUnit unit, IMapTile tile);

        #endregion

    }

}
