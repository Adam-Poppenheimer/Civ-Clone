using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class RiverTriangulator : IRiverTriangulator {

        #region instance fields and properties

        public IHexMesh RiverSurfaceMesh {
            get { return Grid.RiverSurfaceMesh; }
        }

        public IHexMesh RiverBankMesh {
            get { return Grid.RiverBankMesh; }
        }



        private IMapRenderConfig          RenderConfig;
        private IRiverSplineBuilder       RiverSplineBuilder;
        private INoiseGenerator           NoiseGenerator;
        private ICellEdgeContourCanon     CellEdgeContourCanon;
        private INonRiverContourBuilder   NonRiverContourBuilder;
        private IHexGrid                  Grid;
        private IRiverContourRationalizer RiverContourRationalizer;

        #endregion

        #region constructors

        [Inject]
        public RiverTriangulator(
            IMapRenderConfig renderConfig, IRiverSplineBuilder riverSplineBuilder,
            INoiseGenerator noiseGenerator, ICellEdgeContourCanon cellEdgeContourCanon,
            INonRiverContourBuilder nonRiverContourBuilder, IHexGrid grid,
            IRiverContourRationalizer riverContourCuller
        ) {
            RenderConfig             = renderConfig;
            RiverSplineBuilder       = riverSplineBuilder;
            NoiseGenerator           = noiseGenerator;
            CellEdgeContourCanon     = cellEdgeContourCanon;
            NonRiverContourBuilder   = nonRiverContourBuilder;
            Grid                     = grid;
            RiverContourRationalizer = riverContourCuller;
        }

        #endregion

        #region instance methods

        #region from IRiverTriangulator

        public void TriangulateRivers() {
            Profiler.BeginSample("TriangulateRivers()");

            RiverSplineBuilder.RefreshRiverSplines();

            RiverSurfaceMesh.Clear();
            RiverBankMesh   .Clear();

            foreach(var riverSpline in RiverSplineBuilder.LastBuiltRiverSplines) {
                Vector3 riverYVector = Vector3.up * RenderConfig.WaterY;

                var centerSpline = riverSpline.CenterSpline;

                float maxInnerWidth = RenderConfig.RiverMaxInnerWidth * Mathf.Clamp01((float)centerSpline.CurveCount / RenderConfig.RiverCurvesForMaxWidth);

                float maxOuterWidth = (RenderConfig.RiverMaxInnerWidth + RenderConfig.RiverBankWidth) *
                                      Mathf.Clamp01((float)centerSpline.CurveCount / RenderConfig.RiverCurvesForMaxWidth);

                int riverQuadCount = centerSpline.CurveCount * RenderConfig.RiverQuadsPerCurve;

                float tDelta = 1f / riverQuadCount;

                float t = 0f;

                Vector3 innerPortV1, innerStarboardV1, outerPortV1, outerStarboardV1, point1;

                Vector3 point2 = centerSpline.GetPoint(tDelta) + riverYVector;

                float noise = NoiseGenerator.SampleNoise(
                    new Vector2(point2.x, point2.z), RenderConfig.GenericNoiseSource, RenderConfig.RiverWidthNoise, NoiseType.NegativeOneToOne
                ).x;

                Vector3 flow1 = centerSpline.GetDirection(t);
                Vector3 flow2 = centerSpline.GetDirection(tDelta);

                float innerWidth = maxInnerWidth * (Mathf.Clamp01(tDelta * RenderConfig.RiverWideningRate) + tDelta * noise);
                float outerWidth = maxOuterWidth * (Mathf.Clamp01(tDelta * RenderConfig.RiverWideningRate) + tDelta * noise);

                Vector3 innerPortV2      = point2 + centerSpline.GetNormalXZ(tDelta) * innerWidth;
                Vector3 innerStarboardV2 = point2 - centerSpline.GetNormalXZ(tDelta) * innerWidth;

                Vector3 outerPortV2      = point2 + centerSpline.GetNormalXZ(tDelta) * outerWidth;
                Vector3 outerStarboardV2 = point2 - centerSpline.GetNormalXZ(tDelta) * outerWidth;

                RiverSurfaceMesh.AddTriangle   (centerSpline.Points[0] + riverYVector, innerPortV2, innerStarboardV2);
                RiverSurfaceMesh.AddTriangleUV3(flow1,                                 flow2,  flow2);

                RiverSurfaceMesh.AddTriangleColor(RenderConfig.RiverWaterColor);

                RiverBankMesh.AddTriangle(centerSpline.Points[0] + riverYVector, outerPortV2, outerStarboardV2);
                RiverBankMesh.AddTriangleUV(Vector2.one, Vector2.zero, Vector2.zero);

                for(int sectionIndex = 0; sectionIndex < riverSpline.WesternCells.Count; sectionIndex++) {
                    List<Vector2> thisEdgeContour     = new List<Vector2>();
                    List<Vector2> neighborEdgeContour = new List<Vector2>();

                    if(sectionIndex == 0) {
                        thisEdgeContour    .Add(new Vector2(centerSpline.Points[0].x, centerSpline.Points[0].z));
                        neighborEdgeContour.Add(new Vector2(centerSpline.Points[0].x, centerSpline.Points[0].z));
                    }

                    var sectionFlow = riverSpline.Flows[sectionIndex];

                    for(int pointIndex = 0; pointIndex < RenderConfig.RiverQuadsPerCurve; pointIndex++) {
                        t += tDelta;

                        innerPortV1      = innerPortV2;
                        innerStarboardV1 = innerStarboardV2;

                        outerPortV1      = outerPortV2;
                        outerStarboardV1 = outerStarboardV2;

                        point1 = point2;

                        point2 = centerSpline.GetPoint(t) + riverYVector;

                        noise = NoiseGenerator.SampleNoise(
                            new Vector2(point2.x, point2.z), RenderConfig.GenericNoiseSource, RenderConfig.RiverWidthNoise, NoiseType.NegativeOneToOne
                        ).x;

                        innerWidth = maxInnerWidth * (Mathf.Clamp01(t * RenderConfig.RiverWideningRate) + t * noise);
                        outerWidth = maxOuterWidth * (Mathf.Clamp01(t * RenderConfig.RiverWideningRate) + t * noise);

                        flow1 = flow2;
                        flow2 = centerSpline.GetDirection(t);

                        innerPortV2      = point2 + centerSpline.GetNormalXZ(t) * innerWidth;
                        innerStarboardV2 = point2 - centerSpline.GetNormalXZ(t) * innerWidth;

                        outerPortV2      = point2 + centerSpline.GetNormalXZ(t) * outerWidth;
                        outerStarboardV2 = point2 - centerSpline.GetNormalXZ(t) * outerWidth;

                        RiverSurfaceMesh.AddQuad   (innerPortV1, innerStarboardV1, innerPortV2, innerStarboardV2);
                        RiverSurfaceMesh.AddQuadUV3(flow1,       flow1,            flow2,       flow2);

                        RiverSurfaceMesh.AddQuadColor(RenderConfig.RiverWaterColor);

                        RiverBankMesh.AddQuad  (point1,      point2,      outerPortV1,  outerPortV2);
                        RiverBankMesh.AddQuadUV(Vector2.one, Vector2.one, Vector2.zero, Vector2.zero);

                        RiverBankMesh.AddQuad  (point2,      point1,      outerStarboardV2, outerStarboardV1);
                        RiverBankMesh.AddQuadUV(Vector2.one, Vector2.one, Vector2.zero,     Vector2.zero);

                        thisEdgeContour    .Add(sectionFlow == RiverFlow.Counterclockwise ? innerPortV1     .ToXZ() : innerStarboardV1.ToXZ());
                        neighborEdgeContour.Add(sectionFlow == RiverFlow.Counterclockwise ? innerStarboardV1.ToXZ() : innerPortV1     .ToXZ());
                    }

                    thisEdgeContour    .Add(sectionFlow == RiverFlow.Counterclockwise ? innerPortV2     .ToXZ() : innerStarboardV2.ToXZ());
                    neighborEdgeContour.Add(sectionFlow == RiverFlow.Counterclockwise ? innerStarboardV2.ToXZ() : innerPortV2     .ToXZ());

                    CellEdgeContourCanon.SetContourForCellEdge(
                        riverSpline.WesternCells[sectionIndex], riverSpline.Directions[sectionIndex], thisEdgeContour
                    );

                    CellEdgeContourCanon.SetContourForCellEdge(
                        riverSpline.EasternCells[sectionIndex], riverSpline.Directions[sectionIndex].Opposite(), neighborEdgeContour
                    );
                }
            }

            foreach(var cell in Grid.Cells) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    NonRiverContourBuilder.BuildNonRiverContour(cell, direction);
                }
            }

            foreach(var cell in Grid.Cells) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    RiverContourRationalizer.RationalizeRiverContoursInCorner(cell, direction);
                }
            }

            RiverSurfaceMesh.Apply();
            RiverBankMesh   .Apply();

            Profiler.EndSample();
        }

        #endregion

        #endregion

    }

}
