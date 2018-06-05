using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IWaterTriangulator {

        #region methods

        bool ShouldTriangulateWater(CellTriangulationData data);
        void TriangulateWater      (CellTriangulationData data);

        #endregion

    }

}