using Assets.Simulation.HexMap;
using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IRiverTriangulator {

        #region methods

        void TriangulateCellRivers(IHexCell center, Transform localTransform, IHexMesh mesh);
        void TriangulateRiverCorner(IHexCell center, IHexCell left, IHexCell right, HexDirection direction, Transform localTransform, IHexMesh mesh);
        void TriangulateRiverEdge(IHexCell center, IHexCell right, HexDirection direction, Transform localTransform, IHexMesh mesh);

        #endregion

    }

}