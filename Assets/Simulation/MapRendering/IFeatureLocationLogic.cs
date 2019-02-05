using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IFeatureLocationLogic {

        #region methods

        IEnumerable<Vector3> GetCenterFeaturePoints(
            IHexCell cell
        );

        IEnumerable<Vector3> GetDirectionalFeaturePoints(
            IHexCell cell, HexDirection direction
        );

        #endregion

    }

}
