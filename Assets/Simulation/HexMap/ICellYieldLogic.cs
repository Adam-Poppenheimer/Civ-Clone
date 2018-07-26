using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface ICellYieldLogic {

        #region methods

        YieldSummary GetYieldOfCell(IHexCell cell);

        #endregion

    }

}
