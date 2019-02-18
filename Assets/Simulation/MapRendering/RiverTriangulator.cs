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

        private IMapRenderConfig MapRenderConfig;
        private IRiverCanon      RiverCanon;
        private IHexGrid         Grid;

        #endregion

        #region constructors

        [Inject]
        public RiverTriangulator(IMapRenderConfig mapRenderConfig, IRiverCanon riverCanon, IHexGrid grid) {
            MapRenderConfig = mapRenderConfig;
            RiverCanon      = riverCanon;
            Grid            = grid;
        }

        #endregion

        #region instance methods

        #region from IRiverTriangulator

        public void TriangulateCellRivers(IHexCell center, Transform localTransform, IHexMesh mesh) {
            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                if(direction > HexDirection.SE) {
                    break;
                }

                IHexCell right = Grid.GetNeighbor(center, direction);

                if(right == null) {
                    continue;
                }

                bool hasCenterRightRiver = RiverCanon.HasRiverAlongEdge(center, direction);

                if(hasCenterRightRiver) {
                    TriangulateRiverEdge(center, right, direction, localTransform, mesh);
                }

                if(direction > HexDirection.E) {
                    break;
                }

                IHexCell left = Grid.GetNeighbor(center, direction.Previous());

                if(left == null) {
                    continue;
                }

                bool hasCenterLeftRiver = RiverCanon.HasRiverAlongEdge(center, direction.Previous());
                bool hasLeftRightRiver  = RiverCanon.HasRiverAlongEdge(right,  direction.Previous2());

                if( !(center.Terrain.IsWater() ||  left.Terrain.IsWater() || right.Terrain.IsWater()) &&
                    (hasCenterRightRiver || hasCenterLeftRiver || hasLeftRightRiver)
                ) {
                    TriangulateRiverCorner(center, left, right, direction, localTransform, mesh);
                }
            }
        }

        public void TriangulateRiverEdge(
            IHexCell center, IHexCell right, HexDirection direction, Transform localTransform, IHexMesh mesh
        ) {
            Vector3 localCenterPos = localTransform.InverseTransformPoint(center.AbsolutePosition);
            Vector3 localRightPos  = localTransform.InverseTransformPoint(right .AbsolutePosition);

            localCenterPos.y = MapRenderConfig.WaterY;
            localRightPos .y = MapRenderConfig.WaterY;

            Vector3 flowVector = GetFlow(RiverCanon.GetFlowOfRiverAtEdge(center, direction), direction);

            mesh.AddQuad(
                localRightPos  + MapRenderConfig.GetSecondSolidCorner(direction.Opposite()),
                localCenterPos + MapRenderConfig.GetFirstSolidCorner (direction),
                localRightPos  + MapRenderConfig.GetFirstSolidCorner (direction.Opposite()),
                localCenterPos + MapRenderConfig.GetSecondSolidCorner(direction)
            );

            mesh.AddQuadColor(MapRenderConfig.RiverWaterColor);

            mesh.AddQuadUV3_4D(flowVector, flowVector, flowVector, flowVector);
        }

        public void TriangulateRiverCorner(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction, Transform localTransform, IHexMesh mesh
        ) {
            Vector3 localCenterPos = localTransform.InverseTransformPoint(center.AbsolutePosition);
            Vector3 localLeftPos   = localTransform.InverseTransformPoint(left  .AbsolutePosition);
            Vector3 localRightPos  = localTransform.InverseTransformPoint(right .AbsolutePosition);

            localCenterPos.y = MapRenderConfig.WaterY;
            localLeftPos  .y = MapRenderConfig.WaterY;
            localRightPos .y = MapRenderConfig.WaterY;

            mesh.AddTriangle(
                localCenterPos + MapRenderConfig.GetFirstSolidCorner(direction),
                localLeftPos   + MapRenderConfig.GetSecondSolidCorner(direction.Next()),
                localRightPos  + MapRenderConfig.GetFirstSolidCorner (direction.Previous2())
            );

            mesh.AddTriangle(
                localCenterPos + MapRenderConfig.GetFirstSolidCorner(direction),
                localLeftPos   + MapRenderConfig.GetSecondSolidCorner(direction.Next()),
                localRightPos  + MapRenderConfig.GetFirstSolidCorner (direction.Previous2())
            );

            mesh.AddTriangleColor(MapRenderConfig.RiverWaterColor_HalfAlpha);
            mesh.AddTriangleColor(MapRenderConfig.RiverWaterColor_HalfAlpha);

            Vector4 centerLeftFlow  = GetFlow(RiverCanon.GetFlowOfRiverAtEdge(center, direction.Previous()), direction.Previous());
            Vector4 centerRightFlow = GetFlow(RiverCanon.GetFlowOfRiverAtEdge(center, direction), direction);

            mesh.AddTriangleUV3_4D(centerLeftFlow,  centerLeftFlow,  centerLeftFlow);
            mesh.AddTriangleUV3_4D(centerRightFlow, centerRightFlow, centerRightFlow);
        }

        #endregion

        private Vector4 GetFlow(RiverFlow flow, HexDirection direction) {
            return MapRenderConfig.RiverFlowSpeed * (
                flow == RiverFlow.Clockwise
                ? (MapRenderConfig.GetFirstSolidCorner (direction) - MapRenderConfig.GetSecondSolidCorner(direction)).normalized
                : (MapRenderConfig.GetSecondSolidCorner(direction) - MapRenderConfig.GetFirstSolidCorner (direction)).normalized
            );
        }

        #endregion

    }

}
