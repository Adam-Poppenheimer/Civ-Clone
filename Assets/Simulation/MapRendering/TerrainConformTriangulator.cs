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
            if(maxSideLength <= 0.01f) {
                throw new ArgumentOutOfRangeException("maxSideLength", "MaxSideLength too small. Value should be greater than 0.01f");

            }else if(mesh == null) {
                throw new ArgumentNullException("mesh");
            }

            Queue<TriangleData> triangles = new Queue<TriangleData>();

            var startingTriangle = new TriangleData(
                new VertexData(vertex1, uv1, color1), new VertexData(vertex2, uv2, color2), new VertexData(vertex3, uv3, color3)
            );

            triangles.Enqueue(startingTriangle);

            float maxSideLengthSqr = maxSideLength * maxSideLength;

            float abLengthSqr, acLengthSqr, bcLengthSqr, maxLength;

            while(triangles.Count > 0) {
                var nextTriangle = triangles.Dequeue();

                //If the triangles are too big, we subdivide our current triangle into smaller ones,
                //aiming for the expected dimensions of a heightmap "texel."
                //The current construction attempts to create triangles that are as regular as possible
                //and consistently reduce side length. We do this by splitting the triangle along its
                //longest side
                abLengthSqr = (nextTriangle.A.Point - nextTriangle.B.Point).sqrMagnitude;
                acLengthSqr = (nextTriangle.A.Point - nextTriangle.C.Point).sqrMagnitude;
                bcLengthSqr = (nextTriangle.B.Point - nextTriangle.C.Point).sqrMagnitude;

                maxLength = Mathf.Max(abLengthSqr, acLengthSqr, bcLengthSqr);

                if( abLengthSqr > maxSideLengthSqr ||
                    acLengthSqr > maxSideLengthSqr ||
                    bcLengthSqr > maxSideLengthSqr
                ) {
                    if(abLengthSqr == maxLength) {
                        VertexData AB = new VertexData(
                            (nextTriangle.A.Point + nextTriangle.B.Point) / 2f,
                            (nextTriangle.A.UV    + nextTriangle.B.UV   ) / 2f,
                            Color.Lerp(nextTriangle.A.Color, nextTriangle.B.Color, 0.5f)
                        );

                        triangles.Enqueue(new TriangleData(nextTriangle.A, AB,             nextTriangle.C));
                        triangles.Enqueue(new TriangleData(AB,             nextTriangle.B, nextTriangle.C));

                    }else if(acLengthSqr == maxLength) {
                        VertexData AC = new VertexData(
                            (nextTriangle.A.Point + nextTriangle.C.Point) / 2f,
                            (nextTriangle.A.UV    + nextTriangle.C.UV   ) / 2f,
                            Color.Lerp(nextTriangle.A.Color, nextTriangle.C.Color, 0.5f)
                        );

                        triangles.Enqueue(new TriangleData(nextTriangle.A, nextTriangle.B, AC            ));
                        triangles.Enqueue(new TriangleData(AC,             nextTriangle.B, nextTriangle.C));

                    }else {
                        VertexData BC = new VertexData(
                            (nextTriangle.B.Point + nextTriangle.C.Point) / 2f,
                            (nextTriangle.B.UV    + nextTriangle.C.UV   ) / 2f,
                            Color.Lerp(nextTriangle.B.Color, nextTriangle.C.Color, 0.5f)
                        );

                        triangles.Enqueue(new TriangleData(nextTriangle.A, nextTriangle.B, BC            ));
                        triangles.Enqueue(new TriangleData(nextTriangle.A, BC,             nextTriangle.C));
                    }
                } else {
                    Vector3 aOnMap = MapCollisionLogic.GetNearestMapPointToPoint(nextTriangle.A.Point);
                    Vector3 bOnMap = MapCollisionLogic.GetNearestMapPointToPoint(nextTriangle.B.Point);
                    Vector3 cOnMap = MapCollisionLogic.GetNearestMapPointToPoint(nextTriangle.C.Point);

                    mesh.AddTriangle     (aOnMap,               bOnMap,               cOnMap);
                    mesh.AddTriangleUV   (nextTriangle.A.UV,    nextTriangle.B.UV,    nextTriangle.C.UV);
                    mesh.AddTriangleColor(nextTriangle.A.Color, nextTriangle.B.Color, nextTriangle.C.Color);
                }
            }
        }

        #endregion

        #endregion

    }

}
