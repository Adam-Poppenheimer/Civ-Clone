using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public interface IUnitTerrainCostLogic {

        #region methods

        int GetCostToMoveUnitIntoTile(IUnit unit, IHexCell tile);

        #endregion

    }

}
