using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public interface IUnitLineOfSightLogic {

        #region methods

        IEnumerable<IHexCell> GetCellsVisibleToUnit(IUnit unit);

        #endregion

    }

}
