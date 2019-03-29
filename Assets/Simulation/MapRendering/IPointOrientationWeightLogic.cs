using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IPointOrientationWeightLogic {

        #region methods

        void ApplyLandBesideLandWeights (Vector2 xzPoint, PointOrientationData data, bool hasPreviousRiver, bool hasNextRiver);
        void ApplyLandBesideRiverWeights(Vector2 xzPoint, PointOrientationData data);
        void ApplyRiverWeights          (Vector2 xzPoint, PointOrientationData data);

        void GetRiverCornerWeights(
            Vector2 xzPoint, IHexCell center, IHexCell left, IHexCell right, HexDirection sextant,
            out float centerWeight, out float leftWeight, out float rightWeight, out float riverHeightWeight,
            out float riverAlphaWeight
        );

        #endregion

    }

}