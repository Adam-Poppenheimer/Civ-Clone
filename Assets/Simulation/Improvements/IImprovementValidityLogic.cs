using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementValidityLogic {

        #region methods

        bool IsTemplateValidForTile(IImprovementTemplate template, IHexCell tile);

        #endregion

    }

}
