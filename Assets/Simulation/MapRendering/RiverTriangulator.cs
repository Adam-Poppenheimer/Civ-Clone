using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class RiverTriangulator : IRiverTriangulator {

        #region instance fields and properties

        private IMapRenderConfig    RenderConfig;
        private IRiverSplineBuilder RiverSplineBuilder;
        private IHexMesh            RiverMesh;

        #endregion

        #region constructors

        [Inject]
        public RiverTriangulator(
            IMapRenderConfig renderConfig, IRiverSplineBuilder riverSplineBuilder,
            [Inject(Id = "River Mesh")] IHexMesh riverMesh
        ) {
            RenderConfig       = renderConfig;
            RiverSplineBuilder = riverSplineBuilder;
            RiverMesh          = riverMesh;
        }

        #endregion

        #region instance methods

        #region from IRiverTriangulator

        public void TriangulateRivers() {
            RiverSplineBuilder.RefreshRiverSplines();

            RiverMesh.Clear();

            foreach(var spline in RiverSplineBuilder.LastBuiltRiverSplines.Select(riverSpline => riverSpline.CenterSpline)) {
                int riverQuadCount = spline.CurveCount * RenderConfig.RiverQuadsPerCurve;

                float tDelta = 1f / riverQuadCount;

                Vector3 portV2      = spline.GetPoint(tDelta) - spline.GetNormalXZ(tDelta) * RenderConfig.RiverWidth;
                Vector3 starboardV2 = spline.GetPoint(tDelta) + spline.GetNormalXZ(tDelta) * RenderConfig.RiverWidth;

                RiverMesh.AddTriangle(spline.Points[0], portV2, starboardV2);
                RiverMesh.AddTriangleColor(RenderConfig.RiverWaterColor);

                for(float t = tDelta; t <= 1f; t += tDelta) {
                    Vector3 portV1      = portV2;
                    Vector3 starboardV1 = starboardV2;

                    portV2      = spline.GetPoint(t) - spline.GetNormalXZ(t) * RenderConfig.RiverWidth;
                    starboardV2 = spline.GetPoint(t) + spline.GetNormalXZ(t) * RenderConfig.RiverWidth;

                    RiverMesh.AddQuad(portV1, starboardV1, portV2, starboardV2);
                    RiverMesh.AddQuadColor(RenderConfig.RiverWaterColor);
                }
            }

            RiverMesh.Apply();
        }

        #endregion

        #endregion

    }

}
