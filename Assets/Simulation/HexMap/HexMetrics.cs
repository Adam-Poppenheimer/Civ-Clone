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

        public static Vector3[] Corners = {
            new Vector3(0f, 0f,  OuterRadius),
            new Vector3(InnerRadius, 0f,  0.5f * OuterRadius),
            new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
            new Vector3(0f, 0f, -OuterRadius),
            new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius),
            new Vector3(-InnerRadius, 0f,  0.5f * OuterRadius),
            new Vector3(0f, 0f, OuterRadius)
        };

        #endregion

    }

}
