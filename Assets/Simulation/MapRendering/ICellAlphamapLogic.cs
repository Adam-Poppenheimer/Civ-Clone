using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface ICellAlphamapLogic {

        #region methods

        float[] GetAlphamapForPointForCell(IHexCell center, HexDirection sextant);

        #endregion

    }

}
