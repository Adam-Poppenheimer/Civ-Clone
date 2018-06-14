using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IFeaturePlacer {

        #region methods

        bool TryPlaceFeatureAtLocation(IHexCell cell, Vector3 location, int locationIndex, HexHash hash);

        #endregion

    }

}