using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public struct EdgeVertices {

        #region instance fields and properties

        public Vector3 V1, V2, V3, V4;

        #endregion

        #region constructors

        public EdgeVertices(Vector3 cornerOne, Vector3 cornerTwo) {
            V1 = cornerOne;
            V2 = Vector3.Lerp(cornerOne, cornerTwo, 1f / 3f);
            V3 = Vector3.Lerp(cornerOne, cornerTwo, 2f / 3f);
            V4 = cornerTwo;
        }

        #endregion

        #region instance methods

        public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step) {
            EdgeVertices result;

            result.V1 = HexMetrics.TerraceLerp(a.V1, b.V1, step);
            result.V2 = HexMetrics.TerraceLerp(a.V2, b.V2, step);
            result.V3 = HexMetrics.TerraceLerp(a.V3, b.V3, step);
            result.V4 = HexMetrics.TerraceLerp(a.V4, b.V4, step);

            return result;
        }

        #endregion

    }

}
