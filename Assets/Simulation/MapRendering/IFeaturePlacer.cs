using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IFeaturePlacer {

        #region methods

        bool TryPlaceFeatureAtLocation(IHexCell cell, Vector3 location, int locationIndex, HexHash hash);

        #endregion

    }

}