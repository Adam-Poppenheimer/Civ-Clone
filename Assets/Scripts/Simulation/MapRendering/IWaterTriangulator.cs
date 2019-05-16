using Assets.Simulation.HexMap;
using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IWaterTriangulator {

        #region methods

        void TriangulateWaterForCell(IHexCell cell, Transform localTransform, IHexMesh mesh);

        #endregion

    }

}