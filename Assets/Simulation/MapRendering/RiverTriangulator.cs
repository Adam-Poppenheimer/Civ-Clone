using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class RiverTriangulator : IRiverTriangulator {

        #region instance fields and properties

        private IMapRenderConfig        RenderConfig;
        private IRiverSplineBuilder     RiverSplineBuilder;
        private IHexMesh                RiverMesh;
        private INoiseGenerator         NoiseGenerator;
        private ICellEdgeContourCanon   CellEdgeContourCanon;
        private INonRiverContourBuilder NonRiverContourBuilder;

        #endregion

        #region constructors

        [Inject]
        public RiverTriangulator(
            IMapRenderConfig renderConfig, IRiverSplineBuilder riverSplineBuilder,
            [Inject(Id = "River Mesh")] IHexMesh riverMesh, INoiseGenerator noiseGenerator,
            ICellEdgeContourCanon cellEdgeContourCanon, INonRiverContourBuilder nonRiverContourBuilder
        ) {
            RenderConfig           = renderConfig;
            RiverSplineBuilder     = riverSplineBuilder;
            RiverMesh              = riverMesh;
            NoiseGenerator         = noiseGenerator;
            CellEdgeContourCanon   = cellEdgeContourCanon;
            NonRiverContourBuilder = nonRiverContourBuilder;
        }

        #endregion

        #region instance methods

        #region from IRiverTriangulator

        public void TriangulateRivers() {
            RiverSplineBuilder.RefreshRiverSplines();

            RiverMesh.Clear();

            foreach(var riverSpline in RiverSplineBuilder.LastBuiltRiverSplines) {
                var centerSpline = riverSpline.CenterSpline;

                float maxWidth = RenderConfig.RiverMaxWidth * Mathf.Clamp01((float)centerSpline.CurveCount / RenderConfig.RiverCurvesForMaxWidth);

                int riverQuadCount = centerSpline.CurveCount * RenderConfig.RiverQuadsPerCurve;

                float tDelta = 1f / riverQuadCount;

                float t = 0f;

                Vector3 portV1, starboardV1;

                Vector3 point = centerSpline.GetPoint(tDelta);

                float noise = NoiseGenerator.SampleNoise(point, NoiseType.Generic).x * 2f - 1f;

                float width2 = maxWidth * (
                    Mathf.Clamp01(tDelta * RenderConfig.RiverWideningRate) + tDelta * noise * RenderConfig.RiverWidthNoise
                );

                Vector3 flow1 = centerSpline.GetDirection(t);
                Vector3 flow2 = centerSpline.GetDirection(tDelta);

                Vector3 portV2      = point + centerSpline.GetNormalXZ(tDelta) * width2;
                Vector3 starboardV2 = point - centerSpline.GetNormalXZ(tDelta) * width2;

                RiverMesh.AddTriangle   (centerSpline.Points[0], portV2, starboardV2);
                RiverMesh.AddTriangleUV3(flow1,                  flow2,  flow2);

                RiverMesh.AddTriangleColor(RenderConfig.RiverWaterColor);

                for(int sectionIndex = 0; sectionIndex < riverSpline.WesternCells.Count; sectionIndex++) {
                    List<Vector3> thisEdgeContour     = new List<Vector3>();
                    List<Vector3> neighborEdgeContour = new List<Vector3>();

                    if(sectionIndex == 0) {
                        thisEdgeContour    .Add(centerSpline.Points[0]);
                        neighborEdgeContour.Add(centerSpline.Points[0]);
                    }

                    var sectionFlow = riverSpline.Flows[sectionIndex];

                    for(int pointIndex = 0; pointIndex < RenderConfig.RiverQuadsPerCurve; pointIndex++) {
                        t += tDelta;

                        portV1      = portV2;
                        starboardV1 = starboardV2;

                        point = centerSpline.GetPoint(t);

                        noise = NoiseGenerator.SampleNoise(point, NoiseType.Generic).x * 2f - 1f;

                        width2 = maxWidth * (
                            Mathf.Clamp01(t * RenderConfig.RiverWideningRate) + t * noise * RenderConfig.RiverWidthNoise
                        );

                        flow1 = flow2;
                        flow2 = centerSpline.GetDirection(t);

                        portV2      = point + centerSpline.GetNormalXZ(t) * width2;
                        starboardV2 = point - centerSpline.GetNormalXZ(t) * width2;

                        RiverMesh.AddQuad   (portV1, starboardV1, portV2, starboardV2);
                        RiverMesh.AddQuadUV3(flow1,  flow1,       flow2,  flow2);

                        RiverMesh.AddQuadColor(RenderConfig.RiverWaterColor);

                        thisEdgeContour    .Add(sectionFlow == RiverFlow.Counterclockwise ? portV1      : starboardV1);
                        neighborEdgeContour.Add(sectionFlow == RiverFlow.Counterclockwise ? starboardV1 : portV1);
                    }

                    thisEdgeContour    .Add(sectionFlow == RiverFlow.Counterclockwise ? portV2      : starboardV2);
                    neighborEdgeContour.Add(sectionFlow == RiverFlow.Counterclockwise ? starboardV2 : portV2);

                    neighborEdgeContour.Reverse();

                    CellEdgeContourCanon.SetContourForCellEdge(
                        riverSpline.WesternCells[sectionIndex], riverSpline.Directions[sectionIndex], thisEdgeContour
                    );

                    CellEdgeContourCanon.SetContourForCellEdge(
                        riverSpline.EasternCells[sectionIndex], riverSpline.Directions[sectionIndex].Opposite(), neighborEdgeContour
                    );
                }
            }

            NonRiverContourBuilder.BuildNonRiverContours();

            RiverMesh.Apply();
        }

        #endregion

        #endregion

    }

}
