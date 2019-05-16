using Assets.Simulation.HexMap;
using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IContourTriangulator {

        #region methods

        void TriangulateContoursBetween(
            IHexCell center, IHexCell right, HexDirection direction,
            Color centerWeights, Color rightWeight, IHexMesh mesh
        );

        #endregion

    }

}