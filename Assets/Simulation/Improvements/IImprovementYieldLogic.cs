using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementYieldLogic {

        #region methods

        ResourceSummary GetYieldOfImprovement(IImprovement improvement);

        ResourceSummary GetExpectedYieldOfImprovementOnCell(IImprovementTemplate template, IHexCell cell);

        #endregion

    }

}
