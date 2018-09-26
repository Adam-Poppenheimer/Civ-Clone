using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IHexEdgeTypeLogic {

        #region methods

        HexEdgeType GetEdgeTypeBetweenCells(IHexCell cellOne, IHexCell cellTwo);

        #endregion

    }

}
