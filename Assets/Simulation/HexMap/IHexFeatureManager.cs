using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexFeatureManager {

        #region methods

        void Apply();
        void Clear();

        void AddFeatureLocationsForCell(IHexCell cell);

        #endregion

    }

}