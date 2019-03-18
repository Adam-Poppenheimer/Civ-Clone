using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class RiverTriangulator : IRiverTriangulator {

        #region instance fields and properties

        private IMapRenderConfig          RenderConfig;
        private IRiverSplineBuilder       RiverSplineBuilder;
        private IHexMesh                  RiverMesh;
        private INoiseGenerator           NoiseGenerator;
        private ICellEdgeContourCanon     CellEdgeContourCanon;
        private INonRiverContourBuilder   NonRiverContourBuilder;
        private IRiverContourRationalizer RiverContourRationalizer;
        private IHexGrid                  Grid;

        #endregion

        #region constructors

        [Inject]
        public RiverTriangulator(
            IMapRenderConfig renderConfig, IRiverSplineBuilder riverSplineBuilder,
            [Inject(Id = "River Mesh")] IHexMesh riverMesh, INoiseGenerator noiseGenerator,
            ICellEdgeContourCanon cellEdgeContourCanon, INonRiverContourBuilder nonRiverContourBuilder,
            IRiverContourRationalizer riverContourCuller, IHexGrid grid
        ) {
            RenderConfig             = renderConfig;
            RiverSplineBuilder       = riverSplineBuilder;
            RiverMesh                = riverMesh;
            NoiseGenerator           = noiseGenerator;
            CellEdgeContourCanon     = cellEdgeContourCanon;
            NonRiverContourBuilder   = nonRiverContourBuilder;
            RiverContourRationalizer = riverContourCuller;
            Grid                     = grid;
        }

        #endregion

        #region instance methods

        #region from IRiverTriangulator

        public void TriangulateRivers() {
            RiverSplineBuilder.RefreshRiverSplines();

            RiverMesh.Clear();

            foreach(var riverSpline in RiverSplineBuilder.LastBuiltRiverSplines) {
                Vector3 riverYVector = Vector3.up * RenderConfig.WaterY;

                var centerSpline = riverSpline.CenterSpline;

                float maxWidth = RenderConfig.RiverMaxWidth * Mathf.Clamp01((float)centerSpline.CurveCount / RenderConfig.RiverCurvesForMaxWidth);

                int riverQuadCount = centerSpline.CurveCount * RenderConfig.RiverQuadsPerCurve;

                float tDelta = 1f / riverQuadCount;

                float t = 0f;

                Vector3 portV1, starboardV1;

                Vector3 point = centerSpline.GetPoint(tDelta) + riverYVector;

                float noise = NoiseGenerator.SampleNoise(
                    new Vector2(point.x, point.z), RenderConfig.GenericNoiseSource, RenderConfig.RiverWidthNoise, NoiseType.NegativeOneToOne
                ).x;

                Vector3 flow1 = centerSpline.GetDirection(t);
                Vector3 flow2 = centerSpline.GetDirection(tDelta);

                float width2 = maxWidth * (
                    Mathf.Clamp01(tDelta * RenderConfig.RiverWideningRate) + tDelta * noise
                );

                Vector3 portV2      = point + centerSpline.GetNormalXZ(tDelta) * width2;
                Vector3 starboardV2 = point - centerSpline.GetNormalXZ(tDelta) * width2;

                Vector2 portV1XZ, starboardV1XZ;

                Vector2 portV2XZ      = new Vector2(portV2     .x, portV2     .z);
                Vector2 starboardV2XZ = new Vector2(starboardV2.x, starboardV2.z);

                RiverMesh.AddTriangle   (centerSpline.Points[0] + riverYVector, portV2, starboardV2);
                RiverMesh.AddTriangleUV3(flow1,                                 flow2,  flow2);

                RiverMesh.AddTriangleColor(RenderConfig.RiverWaterColor);

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

                        portV1      = portV2;
                        starboardV1 = starboardV2;

                        portV1XZ      = portV2XZ;
                        starboardV1XZ = starboardV2XZ;

                        point = centerSpline.GetPoint(t) + riverYVector;

                        noise = NoiseGenerator.SampleNoise(
                            new Vector2(point.x, point.z), RenderConfig.GenericNoiseSource, RenderConfig.RiverWidthNoise, NoiseType.NegativeOneToOne
                        ).x;

                        width2 = maxWidth * (
                            Mathf.Clamp01(t * RenderConfig.RiverWideningRate) + t * noise
                        );

                        flow1 = flow2;
                        flow2 = centerSpline.GetDirection(t);

                        portV2      = point + centerSpline.GetNormalXZ(t) * width2;
                        starboardV2 = point - centerSpline.GetNormalXZ(t) * width2;

                        portV2XZ      = new Vector2(portV2     .x, portV2     .z);
                        starboardV2XZ = new Vector2(starboardV2.x, starboardV2.z);

                        RiverMesh.AddQuad   (portV1, starboardV1, portV2, starboardV2);
                        RiverMesh.AddQuadUV3(flow1,  flow1,       flow2,  flow2);

                        RiverMesh.AddQuadColor(RenderConfig.RiverWaterColor);

                        thisEdgeContour    .Add(sectionFlow == RiverFlow.Counterclockwise ? portV1XZ      : starboardV1XZ);
                        neighborEdgeContour.Add(sectionFlow == RiverFlow.Counterclockwise ? starboardV1XZ : portV1XZ);
                    }

                    thisEdgeContour    .Add(sectionFlow == RiverFlow.Counterclockwise ? portV2XZ      : starboardV2XZ);
                    neighborEdgeContour.Add(sectionFlow == RiverFlow.Counterclockwise ? starboardV2XZ : portV2XZ);

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

            RiverMesh.Apply();
        }

        #endregion

        #endregion

    }

}
