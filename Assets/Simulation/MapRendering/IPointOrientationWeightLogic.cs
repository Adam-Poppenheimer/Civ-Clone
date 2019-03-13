using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IPointOrientationWeightLogic {

        #region methods

        void ApplyLandBesideLandWeights     (Vector2 xzPoint, PointOrientationData data);
        void ApplyLandBesideLandWeights_Edge(Vector2 xzPoint, PointOrientationData data);
        void ApplyLandBesideRiverWeights    (Vector2 xzPoint, PointOrientationData data, bool takeFromRight);
        void ApplyRiverWeights              (Vector2 xzPoint, PointOrientationData data);

        #endregion

    }

}