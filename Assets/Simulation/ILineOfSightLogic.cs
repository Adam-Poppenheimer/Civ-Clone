using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Units;

namespace Assets.Simulation {

    public interface ILineOfSightLogic {

        #region methods

        IEnumerable<IHexCell> GetCellsVisibleToUnit(IUnit unit);

        IEnumerable<IHexCell> GetCellsVisibleToCity(ICity city);

        bool CanUnitSeeCell(IUnit unit, IHexCell cell);

        #endregion

    }

}
