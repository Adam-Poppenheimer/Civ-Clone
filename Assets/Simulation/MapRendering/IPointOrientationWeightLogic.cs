using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IPointOrientationWeightLogic {

        #region methods

        void ApplyLandBesideLandWeights     (Vector2 xzPoint, PointOrientationData data);
        void ApplyLandBesideLandWeights_Edge(Vector2 xzPoint, PointOrientationData data);
        void ApplyLandBesideRiverWeights    (Vector2 xzPoint, PointOrientationData data, bool takeFromRight);
        void ApplyRiverWeights              (Vector2 xzPoint, PointOrientationData data);

        void GetRiverCornerWeights(
            Vector2 xzPoint, IHexCell center, IHexCell left, IHexCell right, HexDirection sextant,
            out float centerWeight, out float leftWeight, out float rightWeight, out float riverWeight
        );

        #endregion

    }

}