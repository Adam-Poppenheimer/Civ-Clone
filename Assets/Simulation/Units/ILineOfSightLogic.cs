﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public interface ILineOfSightLogic {

        #region methods

        bool CanUnitSeeCell(IUnit unit, IHexCell cell);

        IEnumerable<IHexCell> GetCellsVisibleToUnit(IUnit unit);

        #endregion

    }

}
