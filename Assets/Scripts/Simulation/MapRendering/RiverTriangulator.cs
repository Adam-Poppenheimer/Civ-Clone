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
            RiverSplineBuilder.RefreshRiverSplines();

            Grid.RiverSurfaceMesh.Clear();
            Grid.RiverBankMesh   .Clear();
            Grid.RiverDuckMesh   .Clear();

            foreach(var riverSpline in RiverSplineBuilder.LastBuiltRiverSplines) {
                TriangulateRiver(riverSpline);
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

            Grid.RiverSurfaceMesh.Apply();
            Grid.RiverBankMesh   .Apply();
            Grid.RiverDuckMesh   .Apply();
        }

        #endregion

        private void TriangulateRiver(RiverSpline riverSpline) {
            Vector3 riverYVector = Vector3.up * RenderConfig.WaterY;

            var centerSpline = riverSpline.CenterSpline;

            float maxSurfaceWidth = RenderConfig.RiverMaxSurfaceWidth
                                    * Mathf.Clamp01((float)centerSpline.CurveCount / RenderConfig.RiverCurvesForMaxWidth);

            float maxBankWidth = (RenderConfig.RiverMaxSurfaceWidth + RenderConfig.RiverMaxBankWidth)
                                * Mathf.Clamp01((float)centerSpline.CurveCount / RenderConfig.RiverCurvesForMaxWidth);

            int riverQuadCount = centerSpline.CurveCount * RenderConfig.RiverQuadsPerCurve;

            float tDelta = 1f / riverQuadCount;

            float t = 0f;

            Vector3 surfacePortV1, surfaceStarboardV1, bankPortV1, bankStarboardV1, center1, duckPortV1, duckStarboardV1;

            Vector3 center2 = centerSpline.GetPoint(tDelta) + riverYVector;

            float noise = NoiseGenerator.SampleNoise(
                new Vector2(center2.x, center2.z), RenderConfig.GenericNoiseSource, RenderConfig.RiverWidthNoise, NoiseType.NegativeOneToOne
            ).x;

            Vector3 flow1 = centerSpline.GetDirection(t);
            Vector3 flow2 = centerSpline.GetDirection(tDelta);

            float surfaceWidth = maxSurfaceWidth * (Mathf.Clamp01(tDelta * RenderConfig.RiverWideningRate) + tDelta * noise);
            float bankWidth    = maxBankWidth    * (Mathf.Clamp01(tDelta * RenderConfig.RiverWideningRate) + tDelta * noise);

            Vector3 surfacePortV2      = center2 + centerSpline.GetNormalXZ(tDelta) * surfaceWidth;
            Vector3 surfaceStarboardV2 = center2 - centerSpline.GetNormalXZ(tDelta) * surfaceWidth;

            Vector3 bankPortV2      = center2 + centerSpline.GetNormalXZ(tDelta) * bankWidth;
            Vector3 bankStarboardV2 = center2 - centerSpline.GetNormalXZ(tDelta) * bankWidth;

            Grid.RiverSurfaceMesh.AddTriangle   (centerSpline.Points[0] + riverYVector, surfacePortV2, surfaceStarboardV2);
            Grid.RiverSurfaceMesh.AddTriangleUV3(flow1,                                 flow2,         flow2);
            Grid.RiverSurfaceMesh.AddTriangleColor(RenderConfig.RiverWaterColor);

            Grid.RiverBankMesh.AddTriangle  (centerSpline.Points[0] + riverYVector, bankPortV2,   bankStarboardV2);
            Grid.RiverBankMesh.AddTriangleUV(Vector2.one,                           Vector2.zero, Vector2.zero);

            Vector3 duckPortV2, duckStarboardV2;

            BuildDuckStartCap(
                center2, centerSpline, bankPortV2, bankStarboardV2, tDelta, riverYVector, out duckPortV2, out duckStarboardV2
            );

            //Triangulating the main body of the river
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

                    surfacePortV1      = surfacePortV2;
                    surfaceStarboardV1 = surfaceStarboardV2;

                    bankPortV1      = bankPortV2;
                    bankStarboardV1 = bankStarboardV2;
                        
                    duckPortV1      = duckPortV2;
                    duckStarboardV1 = duckStarboardV2;

                    center1 = center2;

                    center2 = centerSpline.GetPoint(t) + riverYVector;

                    noise = NoiseGenerator.SampleNoise(
                        new Vector2(center2.x, center2.z), RenderConfig.GenericNoiseSource, RenderConfig.RiverWidthNoise, NoiseType.NegativeOneToOne
                    ).x;

                    surfaceWidth = maxSurfaceWidth * (Mathf.Clamp01(t * RenderConfig.RiverWideningRate) + t * noise);
                    bankWidth    = maxBankWidth    * (Mathf.Clamp01(t * RenderConfig.RiverWideningRate) + t * noise);

                    flow1 = flow2;
                    flow2 = centerSpline.GetDirection(t);

                    surfacePortV2      = center2 + centerSpline.GetNormalXZ(t) * surfaceWidth;
                    surfaceStarboardV2 = center2 - centerSpline.GetNormalXZ(t) * surfaceWidth;

                    bankPortV2      = center2 + centerSpline.GetNormalXZ(t) * bankWidth;
                    bankStarboardV2 = center2 - centerSpline.GetNormalXZ(t) * bankWidth;

                    duckPortV2      = center2 + centerSpline.GetNormalXZ(t) * RenderConfig.RiverDuckWidth;
                    duckStarboardV2 = center2 - centerSpline.GetNormalXZ(t) * RenderConfig.RiverDuckWidth;

                    Grid.RiverSurfaceMesh.AddQuad   (surfacePortV1, surfaceStarboardV1, surfacePortV2, surfaceStarboardV2);
                    Grid.RiverSurfaceMesh.AddQuadUV3(flow1,         flow1,              flow2,         flow2);
                    Grid.RiverSurfaceMesh.AddQuadColor(RenderConfig.RiverWaterColor);

                    Grid.RiverBankMesh.AddQuad  (center1,     center2,     bankPortV1,   bankPortV2);
                    Grid.RiverBankMesh.AddQuadUV(Vector2.one, Vector2.one, Vector2.zero, Vector2.zero);

                    Grid.RiverBankMesh.AddQuad  (center2,     center1,     bankStarboardV2, bankStarboardV1);
                    Grid.RiverBankMesh.AddQuadUV(Vector2.one, Vector2.one, Vector2.zero,    Vector2.zero);

                    Grid.RiverDuckMesh.AddQuad  (bankPortV1,   bankPortV2,   duckPortV1,   duckPortV2);
                    Grid.RiverDuckMesh.AddQuadUV(Vector2.one,  Vector2.one,  Vector2.zero, Vector2.zero);

                    Grid.RiverDuckMesh.AddQuad  (bankPortV2,  bankPortV1,  bankStarboardV2, bankStarboardV1);
                    Grid.RiverDuckMesh.AddQuadUV(Vector2.one, Vector2.one, Vector2.one,     Vector2.one);

                    Grid.RiverDuckMesh.AddQuad  (bankStarboardV2, bankStarboardV1, duckStarboardV2, duckStarboardV1);
                    Grid.RiverDuckMesh.AddQuadUV(Vector2.one,     Vector2.one,     Vector2.zero,    Vector2.zero);

                    thisEdgeContour    .Add(sectionFlow == RiverFlow.Counterclockwise ? surfacePortV1     .ToXZ() : surfaceStarboardV1.ToXZ());
                    neighborEdgeContour.Add(sectionFlow == RiverFlow.Counterclockwise ? surfaceStarboardV1.ToXZ() : surfacePortV1     .ToXZ());
                }

                thisEdgeContour    .Add(sectionFlow == RiverFlow.Counterclockwise ? surfacePortV2     .ToXZ() : surfaceStarboardV2.ToXZ());
                neighborEdgeContour.Add(sectionFlow == RiverFlow.Counterclockwise ? surfaceStarboardV2.ToXZ() : surfacePortV2     .ToXZ());

                CellEdgeContourCanon.SetContourForCellEdge(
                    riverSpline.WesternCells[sectionIndex], riverSpline.Directions[sectionIndex], thisEdgeContour
                );

                CellEdgeContourCanon.SetContourForCellEdge(
                    riverSpline.EasternCells[sectionIndex], riverSpline.Directions[sectionIndex].Opposite(), neighborEdgeContour
                );
            }

            BuildDuckEndCap(riverSpline, duckPortV2, duckStarboardV2, bankPortV2, bankStarboardV2);
        }

        private void BuildDuckStartCap(
            Vector3 center2, BezierSpline centerSpline, Vector3 bankPortV2, Vector3 bankStarboardV2, float tDelta, Vector3 riverYVector,
            out Vector3 duckPortV2, out Vector3 duckStarboardV2
        ) {
            Grid.RiverDuckMesh.AddTriangle  (centerSpline.Points[0] + riverYVector, bankPortV2,  bankStarboardV2);
            Grid.RiverDuckMesh.AddTriangleUV(Vector2.one,                           Vector2.one, Vector2.one);

            duckPortV2      = center2 + centerSpline.GetNormalXZ(tDelta) * RenderConfig.RiverDuckWidth;
            duckStarboardV2 = center2 - centerSpline.GetNormalXZ(tDelta) * RenderConfig.RiverDuckWidth;

            Grid.RiverDuckMesh.AddTriangle  (centerSpline.Points[0] + riverYVector, duckPortV2,   bankPortV2);
            Grid.RiverDuckMesh.AddTriangleUV(Vector2.one,                           Vector2.zero, Vector2.one);

            Grid.RiverDuckMesh.AddTriangle  (centerSpline.Points[0] + riverYVector, bankStarboardV2, duckStarboardV2);
            Grid.RiverDuckMesh.AddTriangleUV(Vector2.one,                           Vector2.one,     Vector2.zero);

            Vector3 duckCirclePortStart      =  centerSpline.GetNormalXZ (tDelta) * RenderConfig.RiverDuckWidth;
            Vector3 duckCircleStarboardStart = -centerSpline.GetNormalXZ (tDelta) * RenderConfig.RiverDuckWidth;
            Vector3 duckCircleMiddle         = -centerSpline.GetDirection(tDelta) * RenderConfig.RiverDuckWidth;

            Vector3 center = centerSpline.Points[0] + riverYVector;

            Grid.RiverDuckMesh.AddTriangle  (center,      center + duckCirclePortStart, duckPortV2);
            Grid.RiverDuckMesh.AddTriangleUV(Vector2.one, Vector2.zero,                 Vector2.zero);

            Grid.RiverDuckMesh.AddTriangle  (center,      duckStarboardV2, center + duckCircleStarboardStart);
            Grid.RiverDuckMesh.AddTriangleUV(Vector2.one, Vector2.zero,    Vector2.zero);

            Vector3 duckCirclePortV2      = duckCirclePortStart;
            Vector3 duckCircleStarboardV2 = duckCircleStarboardStart;

            float duckCircleIncr = 2f / RenderConfig.RiverQuadsPerCurve;

            for(float duckParam = 0f; duckParam < 1f; duckParam += duckCircleIncr) {
                Vector3 duckCirclePortV1      = duckCirclePortV2;
                Vector3 duckCircleStarboardV1 = duckCircleStarboardV2;

                duckCirclePortV2      = Vector3.Slerp(duckCirclePortStart,      duckCircleMiddle, duckParam);
                duckCircleStarboardV2 = Vector3.Slerp(duckCircleStarboardStart, duckCircleMiddle, duckParam);

                Grid.RiverDuckMesh.AddTriangle  (center,      center + duckCirclePortV2, center + duckCirclePortV1);
                Grid.RiverDuckMesh.AddTriangleUV(Vector2.one, Vector2.zero,              Vector2.zero);

                Grid.RiverDuckMesh.AddTriangle  (center,      center + duckCircleStarboardV1, center + duckCircleStarboardV2);
                Grid.RiverDuckMesh.AddTriangleUV(Vector2.one, Vector2.zero,                   Vector2.zero);
            }

            Grid.RiverDuckMesh.AddTriangle  (center,      center + duckCircleStarboardV2, center + duckCirclePortV2);
            Grid.RiverDuckMesh.AddTriangleUV(Vector2.one, Vector2.zero,                   Vector2.zero);
        }

        private void BuildDuckEndCap(
            RiverSpline riverSpline, Vector3 duckPortV2, Vector3 duckStarboardV2, Vector3 bankPortV2, Vector3 bankStarboardV2
        ) {
            Vector3 duckWrapPortStart = duckPortV2 - bankPortV2;
            Vector3 duckWrapPortEnd = riverSpline.CenterSpline.GetDirection(1f) * RenderConfig.RiverDuckWidth;

            Vector3 duckCircleV2 = duckWrapPortStart;

            float duckCircleIncr = 2f / RenderConfig.RiverQuadsPerCurve;

            for(float circleParam = 0f; circleParam < 1; circleParam += duckCircleIncr) {
                Vector3 duckCircleV1 = duckCircleV2;

                duckCircleV2 = Vector3.Slerp(duckWrapPortStart, duckWrapPortEnd, circleParam);

                Grid.RiverDuckMesh.AddTriangle  (bankPortV2,  bankPortV2 + duckCircleV1, bankPortV2 + duckCircleV2);
                Grid.RiverDuckMesh.AddTriangleUV(Vector2.one, Vector2.zero,              Vector2.zero);
            }

            Grid.RiverDuckMesh.AddTriangle  (bankPortV2,  bankPortV2 + duckCircleV2, bankPortV2 + duckWrapPortEnd);
            Grid.RiverDuckMesh.AddTriangleUV(Vector2.one, Vector2.zero,              Vector2.zero);

            Vector3 duckWrapStarboardStart = duckStarboardV2 - bankStarboardV2;
            Vector3 duckWrapStarboardEnd   = riverSpline.CenterSpline.GetDirection(1f) * RenderConfig.RiverDuckWidth;

            duckCircleV2 = duckWrapPortStart;

            for(float circleParam = 0f; circleParam < 1; circleParam += duckCircleIncr) {
                Vector3 duckCircleV1 = duckCircleV2;

                duckCircleV2 = Vector3.Slerp(duckWrapStarboardStart, duckWrapStarboardEnd, circleParam);

                Grid.RiverDuckMesh.AddTriangle  (bankStarboardV2,  bankStarboardV2 + duckCircleV2, bankStarboardV2 + duckCircleV1);
                Grid.RiverDuckMesh.AddTriangleUV(Vector2.one,      Vector2.zero,                   Vector2.zero);
            }

            Grid.RiverDuckMesh.AddTriangle  (bankStarboardV2,  bankStarboardV2 + duckWrapStarboardEnd, bankStarboardV2 + duckCircleV2);
            Grid.RiverDuckMesh.AddTriangleUV(Vector2.one,      Vector2.zero,                           Vector2.zero);

            Grid.RiverDuckMesh.AddQuad(
                bankPortV2, bankStarboardV2, bankPortV2 + duckWrapPortEnd, bankStarboardV2 + duckWrapStarboardEnd
            );

            Grid.RiverDuckMesh.AddQuadUV(Vector2.one, Vector2.one, Vector2.zero, Vector2.zero);
        }

        #endregion

    }

}
