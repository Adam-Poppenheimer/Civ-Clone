using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public static class HexMetrics {

        #region static fields and properties

        public const float OuterRadius = 10f;
        public const float InnerRadius = OuterRadius * 0.866025404f;

        public const float SolidFactor = 0.8f;
        public const float BlendFactor = 1f - SolidFactor;

        public const float ElevationStep = 3f;

        public const int TerracesPerSlope = 2;

        public const int TerraceSteps = TerracesPerSlope * 2 + 1;

        public const float HorizontalTerraceStepSize = 1f / TerraceSteps;

        public const float VerticalTerraceStepSize = 1f / (TerracesPerSlope + 1);

        public const float CellPerturbStrength = 4f;

        public const float ElevationPerturbStrength = 1.5f;

        private static Vector3[] Corners = {
            new Vector3(0f, 0f,  OuterRadius),
            new Vector3(InnerRadius, 0f,  0.5f * OuterRadius),
            new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
            new Vector3(0f, 0f, -OuterRadius),
            new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius),
            new Vector3(-InnerRadius, 0f,  0.5f * OuterRadius),
            new Vector3(0f, 0f, OuterRadius)
        };

        #endregion

        #region static methods

        public static Vector3 GetFirstCorner(HexDirection direction) {
            return Corners[(int)direction];
        }

        public static Vector3 GetSecondCorner(HexDirection direction) {
            return Corners[(int)direction + 1];
        }

        public static Vector3 GetFirstSolidCorner(HexDirection direction) {
            return Corners[(int)direction] * SolidFactor;
        }

        public static Vector3 GetSecondSolidCorner(HexDirection direction) {
            return Corners[(int)direction + 1] * SolidFactor;
        }

        public static Vector3 GetBridge(HexDirection direction) {
            return (Corners[(int)direction] + Corners[(int)direction + 1]) * BlendFactor;
        }

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step) {
            float horizontalDelta = step * HexMetrics.HorizontalTerraceStepSize;
            a.x += (b.x - a.x) * horizontalDelta;
            a.z += (b.z - a.z) * horizontalDelta;

            float verticalDelta = ((step + 1) / 2) * HexMetrics.VerticalTerraceStepSize;
            a.y += (b.y - a.y) * verticalDelta;

            return a;
        }

        public static Color TerraceLerp(Color a, Color b, int step) {
            float h = step * HexMetrics.HorizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }

        public static HexEdgeType GetEdgeType(int elevationOne, int elevationTwo) {
            if(elevationOne == elevationTwo) {
                return HexEdgeType.Flat;
            }else if(Math.Abs(elevationOne - elevationTwo) == 1) {
                return HexEdgeType.Slope;
            }else {
                return HexEdgeType.Cliff;
            }
        }

        #endregion

    }

}
