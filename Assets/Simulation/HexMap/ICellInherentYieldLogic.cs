using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IInherentCellYieldLogic {

        #region methods

        YieldSummary GetInherentCellYield(IHexCell cell, bool withVegetationCleared);

        #endregion

    }

}
