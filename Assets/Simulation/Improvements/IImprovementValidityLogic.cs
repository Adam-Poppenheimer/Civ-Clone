using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementValidityLogic {

        #region methods

        bool IsTemplateValidForTile(IImprovementTemplate template, IMapTile tile);

        #endregion

    }

}
