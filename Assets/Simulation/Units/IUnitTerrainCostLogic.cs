using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public interface IUnitTerrainCostLogic {

        #region methods

        float GetTraversalCostForUnit(IUnit unit, IHexCell currentCell, IHexCell nextCell);

        #endregion

    }

}
