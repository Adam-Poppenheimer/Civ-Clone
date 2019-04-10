using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class TerrainConformTriangulator : ITerrainConformTriangulator {

        #region internal classes

        private class TriangleData {

            public VertexData A;
            public VertexData B;
            public VertexData C;

            public TriangleData(VertexData a, VertexData b, VertexData c) {
                A = a;
                B = b;
                C = c;
            }

        }

        private class VertexData {

            public Vector3 Point;
            public Vector2 UV;
            public Color   Color;

            public VertexData(Vector3 point, Vector2 uv, Color color) {
                Point = point;
                UV    = uv;
                Color = color;
            }

        }

        #endregion

        #region instance fields and properties

        private IMapCollisionLogic MapCollisionLogic;

        #endregion

        #region constructors

        [Inject]
        public TerrainConformTriangulator(IMapCollisionLogic mapCollisionLogic) {
            MapCollisionLogic = mapCollisionLogic;
        }

        #endregion

        #region instance methods

        #region from ITerrainConformTriangulator

        public void AddConformingQuad(
            Vector3 bottomLeftVertex,  Vector2 bottomLeftUV,  Color bottomLeftColor,
            Vector3 bottomRightVertex, Vector2 bottomRightUV, Color bottomRightColor,
            Vector3 topLeftVertex,     Vector2 topLeftUV,     Color topLeftColor,
            Vector3 topRightVertex,    Vector2 topRightUV,    Color topRightColor,
            float maxSideLength, IHexMesh mesh
        ) {
            AddConformingTriangle(
                bottomLeftVertex,  bottomLeftUV,  bottomLeftColor,
                topLeftVertex,     topLeftUV,     topLeftColor,
                bottomRightVertex, bottomRightUV, bottomRightColor,
                maxSideLength, mesh
            );

            AddConformingTriangle(
                bottomRightVertex, bottomRightUV, bottomRightColor,
                topLeftVertex,     topLeftUV,     topLeftColor,
                topRightVertex,    topRightUV,    topRightColor,
                maxSideLength, mesh
            );
        }

        public void AddConformingTriangle(
            Vector3 vertex1, Vector2 uv1, Color color1,
            Vector3 vertex2, Vector2 uv2, Color color2,
            Vector3 vertex3, Vector2 uv3, Color color3,
            float maxSideLength, IHexMesh mesh
        ) {
            mesh.AddTriangle     (vertex1, vertex2, vertex3);
            mesh.AddTriangleUV   (uv1,     uv2,     uv3);
            mesh.AddTriangleColor(color1,  color2,  color3);
        }

        #endregion

        #endregion

    }

}
