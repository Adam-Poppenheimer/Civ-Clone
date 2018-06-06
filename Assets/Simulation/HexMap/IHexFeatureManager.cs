using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexFeatureManager {

        #region methods

        void Apply();
        void Clear();

        void FlagLocationForFeatures(Vector3 location, IHexCell cell);

        #endregion

    }

}