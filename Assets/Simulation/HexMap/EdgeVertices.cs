using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public struct EdgeVertices {

        #region instance fields and properties

        public Vector3 V1, V2, V3, V4, V5;

        #endregion

        #region constructors

        public EdgeVertices(Vector3 cornerOne, Vector3 cornerTwo) {
            V1 = cornerOne;
            V2 = Vector3.Lerp(cornerOne, cornerTwo, 0.25f);
            V3 = Vector3.Lerp(cornerOne, cornerTwo, 0.5f);
            V4 = Vector3.Lerp(cornerOne, cornerTwo, 0.75f);
            V5 = cornerTwo;
        }

        public EdgeVertices(Vector3 cornerOne, Vector3 cornerTwo, float outerStep) {
            V1 = cornerOne;
            V2 = Vector3.Lerp(cornerOne, cornerTwo, outerStep);
            V3 = Vector3.Lerp(cornerOne, cornerTwo, 0.5f);
            V4 = Vector3.Lerp(cornerOne, cornerTwo, 1f - outerStep);
            V5 = cornerTwo;
        }

        public EdgeVertices(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5) {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            V4 = v4;
            V5 = v5;
        }

        #endregion

        #region static methods

        public static EdgeVertices LerpFromPoint(Vector3 point, EdgeVertices vertices, float param) {
            return new EdgeVertices(
                Vector3.Lerp(point, vertices.V1, param),
                Vector3.Lerp(point, vertices.V2, param),
                Vector3.Lerp(point, vertices.V3, param),
                Vector3.Lerp(point, vertices.V4, param),
                Vector3.Lerp(point, vertices.V5, param)
            );
        }

        #endregion

    }

}
