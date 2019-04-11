using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class RoadTriangulator : IRoadTriangulator {

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;
        private IHexGrid         Grid;
        private INoiseGenerator  NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public RoadTriangulator(IMapRenderConfig renderConfig, IHexGrid grid, INoiseGenerator noiseGenerator) {
            RenderConfig   = renderConfig;
            Grid           = grid;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IRoadTriangulator

        public void TriangulateRoads(IHexCell cell, IHexMesh roadsMesh) {
            if(cell.HasRoads) {
                var cellHash = NoiseGenerator.SampleHashGrid(cell.AbsolutePositionXZ);

                var directionsWithRoad = EnumUtil.GetValues<HexDirection>().Where(
                    direction => Grid.HasNeighbor(cell, direction) && Grid.GetNeighbor(cell, direction).HasRoads
                ).ToList();

                while(directionsWithRoad.Any()) {
                    var startDirection = directionsWithRoad.First();

                    directionsWithRoad.Remove(startDirection);

                    BezierSpline spline;

                    if(directionsWithRoad.Any()) {
                        var endDirection = GetBestDirection(startDirection, directionsWithRoad, cellHash);

                        directionsWithRoad.Remove(endDirection);

                        spline = BuildSplineBetween(cell, startDirection, endDirection);
                    }else {
                        spline = BuildSplineToCenter(cell, startDirection);
                    }

                    RenderSpline(spline, roadsMesh);
                }
            }
        }

        #endregion

        private HexDirection GetBestDirection(HexDirection startDirection, List<HexDirection> otherDirections, HexHash hash) {
            if(otherDirections.Contains(startDirection.Opposite())) {
                return startDirection.Opposite();

            }else if(otherDirections.Contains(startDirection.Next2())) {
                return startDirection.Next2();

            }else if(otherDirections.Contains(startDirection.Previous2())) {
                return startDirection.Previous2();

            }else {
                return otherDirections[Mathf.FloorToInt(hash.A * otherDirections.Count)];
            }
        }

        private BezierSpline BuildSplineBetween(IHexCell cell, HexDirection directionOne, HexDirection directionTwo) {
            Vector3 controlOne = cell.AbsolutePosition + (
                RenderConfig.GetEdgeMidpoint(directionOne) + RenderConfig.GetEdgeMidpoint(directionOne.Previous())
            ) / 3f;

            Vector3 controlTwo = cell.AbsolutePosition + (
                RenderConfig.GetEdgeMidpoint(directionTwo) + RenderConfig.GetEdgeMidpoint(directionTwo.Previous())
            ) / 3f;

            var spline = new BezierSpline(cell.AbsolutePosition + RenderConfig.GetEdgeMidpoint(directionOne));

            spline.AddCubicCurve(controlOne, controlTwo, cell.AbsolutePosition + RenderConfig.GetEdgeMidpoint(directionTwo));

            return spline;
        }

        private BezierSpline BuildSplineToCenter(IHexCell cell, HexDirection fromEdge) {
            Vector3 start = cell.AbsolutePosition + RenderConfig.GetEdgeMidpoint(fromEdge);

            Vector3 controlOne = cell.AbsolutePosition + (
                RenderConfig.GetEdgeMidpoint(fromEdge) + RenderConfig.GetEdgeMidpoint(fromEdge.Previous())
            ) / 3f;

            var roadSpline = new BezierSpline(start);

            roadSpline.AddCubicCurve(controlOne, cell.AbsolutePosition, cell.AbsolutePosition);

            return roadSpline;
        }

        private void RenderSpline(BezierSpline spline, IHexMesh mesh) {
            if(spline == null) {
                return;
            }

            float delta = 1f / RenderConfig.RoadQuadsPerCurve;

            Vector3 lowerLeft, lowerRight, upperLeft, upperRight;

            for(float t = 0f; t < 1f; t += delta) {
                lowerLeft  = spline.GetPoint(t) + spline.GetNormalXZ(t).normalized * RenderConfig.RoadWidth / 2f;
                lowerRight = spline.GetPoint(t) - spline.GetNormalXZ(t).normalized * RenderConfig.RoadWidth / 2f;

                upperLeft  = spline.GetPoint(t + delta) + spline.GetNormalXZ(t + delta).normalized * RenderConfig.RoadWidth / 2f;
                upperRight = spline.GetPoint(t + delta) - spline.GetNormalXZ(t + delta).normalized * RenderConfig.RoadWidth / 2f;

                mesh.AddQuad(lowerLeft, lowerRight, upperLeft, upperRight);
                mesh.AddQuadUV(0f, 1f, t, t + delta);
            }
        }

        #endregion

    }

}
