using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface ICellResourceLogic {

        #region methods

        ResourceSummary GetYieldOfCell(IHexCell cell);

        #endregion

    }

}
