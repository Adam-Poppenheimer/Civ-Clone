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

    public class MarshTriangulator : IMarshTriangulator {

        #region instance fields and properties

        private IHexGrid              Grid;
        private IMapRenderConfig      RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public MarshTriangulator(
            IHexGrid grid, IMapRenderConfig renderConfig
        ) {
            Grid         = grid;
            RenderConfig = renderConfig;
        }

        #endregion

        #region instance methods

        #region from IMarshTriangulator

        public void TriangulateMarshes(IHexCell center, IHexMesh mesh) {
            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                if(center.Vegetation == CellVegetation.Marsh) {
                    TriangulateMarshCenter(center, direction, mesh);
                }

                if(direction > HexDirection.SE) {
                    continue;
                }

                IHexCell right = Grid.GetNeighbor(center, direction);
                IHexCell left  = Grid.GetNeighbor(center, direction.Previous());

                if(right == null) {
                    continue;
                }

                float centerV =                  center.Vegetation == CellVegetation.Marsh ? 1f : 0f;
                float leftV   = left  != null && left  .Vegetation == CellVegetation.Marsh ? 1f : 0f;
                float rightV  = right != null && right .Vegetation == CellVegetation.Marsh ? 1f : 0f;

                if(centerV != 0f || rightV != 0f) {
                    TriangulateMarshEdge(center, right, centerV, rightV, direction, mesh);
                }

                if(direction > HexDirection.E) {
                    continue;
                }

                if(left == null) {
                    continue;
                }

                if(centerV != 0f || leftV != 0f || rightV != 0f) {
                    TriangulateMarshCorner(center, left, right, centerV, leftV, rightV, direction, mesh);
                }
            }
        }

        #endregion

        private void TriangulateMarshCenter(IHexCell center, HexDirection direction, IHexMesh mesh) {
            mesh.AddTriangle(
                center.AbsolutePosition,
                center.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction),
                center.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction)
            );

            mesh.AddTriangleUV(Vector2.one, Vector2.one, Vector2.one);
        }

        private void TriangulateMarshEdge(
            IHexCell center, IHexCell right, float centerV, float rightV, HexDirection direction, IHexMesh mesh
        ) {
            mesh.AddQuad(
                center.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction),
                center.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction),
                right.AbsolutePosition  + RenderConfig.GetSecondSolidCorner(direction.Opposite()),
                right.AbsolutePosition  + RenderConfig.GetFirstSolidCorner (direction.Opposite())
            );

            mesh.AddQuadUV(0f, 0f, centerV, rightV);
        }

        private void TriangulateMarshCorner(
            IHexCell center, IHexCell left, IHexCell right, float centerV, float leftV, float rightV,
            HexDirection direction, IHexMesh mesh
        ) {
            Vector3 centerPoint = center.AbsolutePosition + RenderConfig.GetFirstSolidCorner(direction);
            Vector3 leftPoint   = left.AbsolutePosition   + RenderConfig.GetFirstSolidCorner(direction.Next2());
            Vector3 rightPoint  = right.AbsolutePosition  + RenderConfig.GetFirstSolidCorner(direction.Previous2());

            mesh.AddTriangle(centerPoint, leftPoint, rightPoint);
            mesh.AddTriangleUV(new Vector2(0f, centerV), new Vector2(0f, leftV), new Vector2(0f, rightV));
        }

        #endregion
        
    }

}
