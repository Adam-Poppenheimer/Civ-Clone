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

        public const float SolidFactor = 0.85f;
        public const float BlendFactor = 1f - SolidFactor;

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

        #endregion

    }

}
