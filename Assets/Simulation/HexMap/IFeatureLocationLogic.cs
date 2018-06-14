using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

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
