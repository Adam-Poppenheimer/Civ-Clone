using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IHeightMixingLogic {

        #region methods

        float GetMixForEdgeAtPoint(IHexCell center, IHexCell right, HexDirection direction, Vector3 point);

        float GetMixForPreviousCornerAtPoint(IHexCell center, IHexCell left,  IHexCell right,     HexDirection direction, Vector3 point);
        float GetMixForNextCornerAtPoint    (IHexCell center, IHexCell right, IHexCell nextRight, HexDirection direction, Vector3 point);

        #endregion

    }

}
