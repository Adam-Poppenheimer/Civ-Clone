using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class MarshTriangulator : IMarshTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;

        #endregion

        #region constructors

        [Inject]
        public MarshTriangulator(IHexGridMeshBuilder meshBuilder) {
            MeshBuilder = meshBuilder;
        }

        #endregion

        #region instance methods

        #region from IMarshTriangulator

        public bool ShouldTriangulateMarshCenter(CellTriangulationData data) {
            return data.Center.Vegetation == CellVegetation.Marsh;
        }

        public bool ShouldTriangulateMarshEdge(CellTriangulationData data) {
            return data.Center.Vegetation == CellVegetation.Marsh
                && data.CenterToRightEdgeType == HexEdgeType.Flat
                && (data.Right.Vegetation != CellVegetation.Marsh || data.Direction <= HexDirection.SE);
        }

        public bool ShouldTriangulateMarshCorner(CellTriangulationData data) {
            bool shouldHaveAnyCorner = data.Direction <= HexDirection.E && data.Right != null && data.Left != null;
            bool bothEdgesAreFlat = data.CenterToLeftEdgeType == HexEdgeType.Flat && data.CenterToRightEdgeType == HexEdgeType.Flat;
            
            return shouldHaveAnyCorner && bothEdgesAreFlat && (
                data.Center.Vegetation == CellVegetation.Marsh ||
                data.Left  .Vegetation == CellVegetation.Marsh ||
                data.Right .Vegetation == CellVegetation.Marsh
            );
        }

        public void TriangulateMarshCenter(CellTriangulationData data) {
            MeshBuilder.TriangulateEdgeFan(
                data.CenterPeak, data.CenterToRightEdge, data.Center.Index, 1f,
                MeshBuilder.Marsh
            );
        }

        public void TriangulateMarshEdge(CellTriangulationData data) {
            if(data.Right.Vegetation == CellVegetation.Marsh) {
                MeshBuilder.TriangulateEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, 1f, false,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index, 1f, false,
                    MeshBuilder.Marsh
                );
            }else {
                MeshBuilder.TriangulateEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, 1f, false,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index, 0f, false,
                    MeshBuilder.Marsh
                );
            }
        }

        public void TriangulateMarshCorner(CellTriangulationData data) {
            MeshBuilder.AddTriangle(
                data.CenterCorner, data.Center.Index, MeshBuilder.Weights1,
                data.LeftCorner,   data.Left  .Index, MeshBuilder.Weights2,
                data.RightCorner,  data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.Marsh
            );

            float centerV = data.Center.Vegetation == CellVegetation.Marsh ? 1f : 0f;
            float leftV   = data.Left  .Vegetation == CellVegetation.Marsh ? 1f : 0f;
            float rightV  = data.Right .Vegetation == CellVegetation.Marsh ? 1f : 0f;

            MeshBuilder.Marsh.AddTriangleUV(
                new Vector2(0f, centerV), new Vector2(0f, leftV), new Vector2(0f, rightV)
            );
        }

        #endregion

        #endregion
        
    }

}
