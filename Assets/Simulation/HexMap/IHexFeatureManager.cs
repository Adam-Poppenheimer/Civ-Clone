using System;
using System.Collections.Generic;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexFeatureManager {

        #region methods

        void Apply();
        void Clear();

        void AddFeatureLocationsForCell(IHexCell cell);

        IEnumerable<Vector3> GetFeatureLocationsForCell(IHexCell cell);

        #endregion

    }

}