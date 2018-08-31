using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface ICellScorer {

        #region methods

        float GetScoreOfCell(IHexCell cell);

        #endregion

    }

}
