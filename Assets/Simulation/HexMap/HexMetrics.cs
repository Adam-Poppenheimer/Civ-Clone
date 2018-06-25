using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public static class HexMetrics {

        #region static fields and properties

        public const float OuterToInner = 0.866025404f;
        public const float InnerToOuter = 1f / OuterToInner;

        public const float OuterRadius = 10f;
        public const float InnerRadius = OuterRadius * OuterToInner;

        public const float OuterSolidFactor = 0.85f;
        public const float InnerSolidFactor = 0.45f;
        public const float BlendFactor = 1f - OuterSolidFactor;

        public const float ElevationStep = 2f;

        public const int TerracesPerSlope = 2;

        public const int TerraceSteps = TerracesPerSlope * 2 + 1;

        public const float HorizontalTerraceStepSize = 1f / TerraceSteps;

        public const float VerticalTerraceStepSize = 1f / (TerracesPerSlope + 1);

        public const float CellPerturbStrengthXZ = 0f;//2f;
        public const float CellPerturbStrengthY  = 0f;//2f;

        public const float MinHillPerturbation = 0f;

        public const float ElevationPerturbStrength = 0f;//1.5f;

        public const int ChunkSizeX = 5, ChunkSizeZ = 5;

        public const float StreamBedElevationOffset = -1.5f;

        public const float OceanElevationOffset = -0.5f;
        public const float RiverElevationOffset = -0.25f;

        public const float WaterFactor = 0.6f;
        public const float WaterBlendFactor = 1f - WaterFactor;

        public const float RiverCurveOffsetDefault = 0.15f;

        public const float RiverSlopedCurveLerp = 0.5f;

        public const float RiverEndpointVMax = 0.25f;

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

        public static Vector3 GetFirstOuterSolidCorner(HexDirection direction) {
            return Corners[(int)direction] * OuterSolidFactor;
        }

        public static Vector3 GetSecondOuterSolidCorner(HexDirection direction) {
            return Corners[(int)direction + 1] * OuterSolidFactor;
        }

        public static Vector3 GetFirstInnerSolidCorner(HexDirection direction) {
            return Corners[(int)direction] * InnerSolidFactor;
        }

        public static Vector3 GetSecondInnerSolidCorner(HexDirection direction) {
            return Corners[(int)direction + 1] * InnerSolidFactor;
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

        public static float TerraceLerp(float a, float b, int step) {
            float h = step * HexMetrics.HorizontalTerraceStepSize;
            return Mathf.Lerp(a, b, h);
        }

        public static HexEdgeType GetEdgeType(IHexCell cellOne, IHexCell cellTwo) {
            int elevationOne = cellOne.EdgeElevation;
            int elevationTwo = cellTwo.EdgeElevation;

            if(elevationOne == elevationTwo) {
                return HexEdgeType.Flat;
            }else if(Math.Abs(elevationOne - elevationTwo) == 1) {
                return HexEdgeType.Slope;
            }else {
                return HexEdgeType.Cliff;
            }
        }

        public static Vector3 GetSolidEdgeMiddle(HexDirection direction) {
            return (Corners[(int)direction] + Corners[(int)direction + 1]) * 0.5f * OuterSolidFactor;
        }

        public static Vector3 GetOuterEdgeMiddle(HexDirection direction) {
            return (Corners[(int)direction] + Corners[(int)direction + 1]) * 0.5f;
        }

        public static Vector3 GetFirstWaterCorner(HexDirection direction) {
            return Corners[(int)direction] * WaterFactor;
        }

        public static Vector3 GetSecondWaterCorner(HexDirection direction) {
            return Corners[(int)direction + 1] * WaterFactor;
        }

        public static Vector3 GetWaterBridge(HexDirection direction) {
            return (Corners[(int)direction] + Corners[(int)direction + 1]) * WaterBlendFactor;
        }

        #endregion

    }

}
