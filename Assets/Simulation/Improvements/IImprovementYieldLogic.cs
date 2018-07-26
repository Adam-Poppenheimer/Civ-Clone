using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementYieldLogic {

        #region methods

        YieldSummary GetYieldOfImprovement(IImprovement improvement);

        YieldSummary GetExpectedYieldOfImprovementOnCell(IImprovementTemplate template, IHexCell cell);

        #endregion

    }

}
