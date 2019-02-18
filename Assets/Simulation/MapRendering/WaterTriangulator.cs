using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Extensions;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class WaterTriangulator : IWaterTriangulator {

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;
        private IHexGrid         Grid;

        #endregion

        #region constructors

        [Inject]
        public WaterTriangulator(IMapRenderConfig renderConfig, IHexGrid grid) {
            RenderConfig = renderConfig;
            Grid         = grid;
        }

        #endregion

        #region instance methods

        #region from IWaterTriangulator

        //Center should always be a water cell of some variety.
        //We need to extend water borders into land cells to make
        //sure the water goes all the way up to the shore.
        public void TriangulateWaterForCell(IHexCell center, Transform localTransform, IHexMesh mesh) {
            Vector3 localCenterPos = localTransform.InverseTransformPoint(center.AbsolutePosition);
            localCenterPos.y = RenderConfig.WaterY;

            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                Color centerColor, leftColor, rightColor;

                GetWaterColors(center, direction, out centerColor, out leftColor, out rightColor);

                TriangulateWaterCenter(center, mesh, direction, localCenterPos, centerColor);

                var right = Grid.GetNeighbor(center, direction);

                if(right == null || (right.Terrain.IsWater() && direction > HexDirection.SE)) {
                    continue;
                }

                Vector3 localRightPos = localTransform.InverseTransformPoint(right.AbsolutePosition);
                localRightPos.y = RenderConfig.WaterY;

                if(center.Terrain.IsWater()) {
                    TriangulateWaterEdge(
                        center, localCenterPos, centerColor, right, localRightPos, rightColor,
                        direction, mesh
                    );
                }else if(right.Terrain.IsWater()) {
                    TriangulateWaterEdge(
                        right, localRightPos, rightColor, center, localCenterPos, centerColor,
                        direction.Opposite(), mesh
                    );
                }


                var left = Grid.GetNeighbor(center, direction.Previous());

                //We can't use the normal paradigm for corners because
                //we aren't triangulating from every cell. We need to
                //triangulate every land corner, and then exclude water
                //corners, which this check does.
                if( left == null ||
                    (right.Terrain.IsWater() && direction > HexDirection.E) ||
                    (left .Terrain.IsWater() && direction > HexDirection.SW)
                ) {
                    continue;
                }

                Vector3 localLeftPos = localTransform.InverseTransformPoint(left.AbsolutePosition);
                localLeftPos.y = RenderConfig.WaterY;

                TriangulateWaterCorner(
                    center, localCenterPos, centerColor,
                    right,  localRightPos,  rightColor,
                    left,   localLeftPos,   leftColor,
                    direction, mesh
                );
            }
        }

        #endregion

        private void TriangulateWaterCenter(
            IHexCell center, IHexMesh mesh, HexDirection direction, Vector3 localCell, Color centerColor
        ) {
            mesh.AddTriangle(
                localCell,
                localCell + RenderConfig.GetFirstSolidCorner (direction),
                localCell + RenderConfig.GetSecondSolidCorner(direction)
            );

            mesh.AddTriangleColor(centerColor);

            mesh.AddTriangleUV3_4D(Vector2.zero, Vector2.zero, Vector2.zero);
        }

        private void TriangulateWaterEdge(
            IHexCell center, Vector3 localCenterPos, Color centerColor,
            IHexCell right,  Vector3 localRightPos,  Color rightColor,
            HexDirection direction, IHexMesh mesh
        ) {
            mesh.AddQuad(
                localCenterPos + RenderConfig.GetFirstSolidCorner (direction),
                localCenterPos + RenderConfig.GetSecondSolidCorner(direction),
                localRightPos  + RenderConfig.GetSecondSolidCorner(direction.Opposite()),
                localRightPos  + RenderConfig.GetFirstSolidCorner (direction.Opposite())
            );

            mesh.AddQuadColor(centerColor, rightColor);

            mesh.AddQuadUV3_4D(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
        }

        private void TriangulateWaterCorner(
            IHexCell center, Vector3 localCenterPos, Color centerColor,
            IHexCell right,  Vector3 localRightPos,  Color rightColor,
            IHexCell left,   Vector3 localLeftPos,   Color leftColor,
            HexDirection direction, IHexMesh mesh
        ) {
            mesh.AddTriangle(
                localCenterPos + RenderConfig.GetFirstSolidCorner(direction),
                localLeftPos   + RenderConfig.GetFirstSolidCorner(direction.Next2()),
                localRightPos  + RenderConfig.GetFirstSolidCorner(direction.Previous2())
            );

            mesh.AddTriangleColor(centerColor, leftColor, rightColor);

            mesh.AddTriangleUV3_4D(Vector2.zero, Vector2.zero, Vector2.zero);
        }

        //Since we're often triangulating water that goes up against land,
        //We need to be careful about how we assign colors. There are three
        //basic cases: center is water, either left or right (but not both)
        //is water, or all three cells are water.
        private void GetWaterColors(
            IHexCell center, HexDirection direction, out Color centerColor, out Color leftColor, out Color rightColor
        ) {
            centerColor = GetWaterColor(center);

            var left  = Grid.GetNeighbor(center, direction.Previous());
            var right = Grid.GetNeighbor(center, direction);

            bool isLeftWater  = left  != null && left .Terrain.IsWater();
            bool isRightWater = right != null && right.Terrain.IsWater();

            if(isLeftWater) {
                leftColor  = GetWaterColor(left);

                rightColor = isRightWater
                           ? rightColor = GetWaterColor(right)
                           : Color.Lerp(centerColor, leftColor, 0.5f);

            }else if(isRightWater) {
                rightColor = GetWaterColor(right);

                leftColor = Color.Lerp(centerColor, rightColor, 0.5f);

            }else {
                leftColor  = centerColor;
                rightColor = centerColor;
            }
        }

        private Color GetWaterColor(IHexCell cell) {
            switch(cell.Terrain) {
                case CellTerrain.ShallowWater: return RenderConfig.ShallowWaterColor;
                case CellTerrain.DeepWater:    return RenderConfig.DeepWaterColor;
                case CellTerrain.FreshWater:   return RenderConfig.FreshWaterColor;
                default:                       return Color.black;
            }
        }

        #endregion

    }

}
