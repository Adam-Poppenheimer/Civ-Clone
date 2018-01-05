using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.HexMap {

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class HexMesh : MonoBehaviour {

        #region static fields and properties

        private static List<Vector3> Vertices  = new List<Vector3>();
        private static List<int>     Triangles = new List<int>();
        private static List<Color>   Colors    = new List<Color>();

        #endregion

        #region instance fields and properties

        private Mesh ManagedMesh;
        public MeshCollider Collider { get; private set; }        

        private IHexGrid Grid;
        private INoiseGenerator NoiseGenerator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IHexGrid grid, INoiseGenerator noiseGenerator) {
            Grid           = grid;
            NoiseGenerator = noiseGenerator;
        }


        #region Unity message methods

        private void Awake() {
            GetComponent<MeshFilter>().mesh = ManagedMesh = new Mesh();
            ManagedMesh.name = "Hex Mesh";
            
            Collider = GetComponent<MeshCollider>();
        }        

        #endregion        

        public void Triangulate(HexCell[] cells) {
            ManagedMesh.Clear();
            Vertices   .Clear();
            Colors     .Clear();
            Triangles  .Clear();

            for(int i = 0; i < cells.Length; ++i) {
                Triangulate(cells[i]);
            }

            ManagedMesh.vertices  = Vertices .ToArray();
            ManagedMesh.triangles = Triangles.ToArray();
            ManagedMesh.colors    = Colors   .ToArray();
            ManagedMesh.RecalculateNormals();

            Collider.sharedMesh = ManagedMesh;
        }

        private void Triangulate(HexCell cell) {
            for(HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; ++direction) {
                Triangulate(direction, cell);
            }
        }

        private void Triangulate(HexDirection direction, HexCell cell) {
            Vector3 center = cell.transform.localPosition;
            EdgeVertices edge = new EdgeVertices(
                center + HexMetrics.GetFirstSolidCorner(direction),
                center + HexMetrics.GetSecondSolidCorner(direction)
            );

            TriangulateEdgeFan(center, edge, cell.Color);

            if(direction <= HexDirection.SE) {
                TriangulateConnection(direction, cell, edge); 
            }                    
        }

        private void TriangulateConnection(
            HexDirection direction, HexCell cell, EdgeVertices edgeOne
        ){
            if(!Grid.HasNeighbor(cell, direction)) {
                return;
            }

            IHexCell neighbor = Grid.GetNeighbor(cell, direction);

            Vector3 bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.transform.localPosition.y - cell.transform.localPosition.y;
            EdgeVertices edgeTwo = new EdgeVertices(
                edgeOne.V1 + bridge,
                edgeOne.V4 + bridge
            );

            var edgeType = HexMetrics.GetEdgeType(cell.Elevation, neighbor.Elevation);
            if(edgeType == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(edgeOne, cell, edgeTwo, neighbor);
            }else {
                TriangulateEdgeStrip(edgeOne, cell.Color, edgeTwo, neighbor.Color);
            }

            if(direction > HexDirection.E || !Grid.HasNeighbor(cell, direction.Next())) {
                return;
            }

            IHexCell nextNeighbor = Grid.GetNeighbor(cell, direction.Next());

            Vector3 v5 = edgeOne.V4 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.transform.localPosition.y;

            if(cell.Elevation <= neighbor.Elevation) {
                if(cell.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCorner(edgeOne.V4, cell, edgeTwo.V4, neighbor, v5, nextNeighbor);
                }else {
                    TriangulateCorner(v5, nextNeighbor, edgeOne.V4, cell, edgeTwo.V4, neighbor);
                }
            }else if(neighbor.Elevation <= nextNeighbor.Elevation) {
                TriangulateCorner(edgeTwo.V4, neighbor, v5, nextNeighbor, edgeOne.V4, cell);
            }else {
                TriangulateCorner(v5, nextNeighbor, edgeOne.V4, cell, edgeTwo.V4, neighbor);
            }

        }

        private void TriangulateEdgeTerraces(
            EdgeVertices begin, IHexCell beginCell,
            EdgeVertices end, IHexCell endCell
        ) {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

            TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices e1 = e2;
                Color c1 = c2;

                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);

                TriangulateEdgeStrip(e1, c1, e2, c2);
            }

            TriangulateEdgeStrip(e2, c2, end, endCell.Color);
        }

        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color) {
            AddTriangle(center, edge.V1, edge.V2);
            AddTriangleColor(color);

            AddTriangle(center, edge.V2, edge.V3);
            AddTriangleColor(color);

            AddTriangle(center, edge.V3, edge.V4);
            AddTriangleColor(color);
        }

        private void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2) {
            AddQuad(e1.V1, e1.V2, e2.V1, e2.V2);
            AddQuadColor(c1, c2);

		    AddQuad(e1.V2, e1.V3, e2.V2, e2.V3);
		    AddQuadColor(c1, c2);

		    AddQuad(e1.V3, e1.V4, e2.V3, e2.V4);
		    AddQuadColor(c1, c2);
        }

        private void TriangulateCorner(
            Vector3 bottom, IHexCell bottomCell,
            Vector3 left,   IHexCell leftCell,
            Vector3 right,  IHexCell rightCell
        ) {
            HexEdgeType leftEdgeType  = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if(leftEdgeType == HexEdgeType.Slope) {

                if(rightEdgeType == HexEdgeType.Slope) {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);

                }else if(rightEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                }else {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                }

            }else if(rightEdgeType == HexEdgeType.Slope) {

                if(leftEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }else {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }

            }else if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {

                if(leftCell.Elevation < rightCell.Elevation) {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }else {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }

            }else {
                AddTriangle(bottom, left, right);
                AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
            }            
        }

        private void TriangulateCornerTerraces(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell
        ){
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);

            Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
            Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

            AddTriangle(begin, v3, v4);
            AddTriangleColor(beginCell.Color, c3, c4);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;

                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);

                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2, c3, c4);
            }

            AddQuad(v3, v4, left, right);
            AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
        }

        private void TriangulateCornerTerracesCliff(
            Vector3 begin, IHexCell beginCell,
            Vector3 left, IHexCell leftCell,
            Vector3 right, IHexCell rightCell
        ){
            float b = Mathf.Abs(1f / (rightCell.Elevation - beginCell.Elevation));

            Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
            Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }else {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        private void TriangulateCornerCliffTerraces(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell
        ){
            float b = Mathf.Abs(1f / (leftCell.Elevation - beginCell.Elevation));
            Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
            Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }else {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        private void TriangulateBoundaryTriangle(
            Vector3 begin, IHexCell beginCell,
            Vector3 left, IHexCell leftCell,
            Vector3 boundary, Color boundaryColor
        ) {
            Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            AddTriangleUnperturbed(Perturb(begin), v2, boundary);
            AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
                c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                AddTriangleUnperturbed(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangleUnperturbed(v2, Perturb(left), boundary);
            AddTriangleColor(c2, leftCell.Color, boundaryColor);
        }

        private Vector3 Perturb(Vector3 position) {
            Vector4 sample = NoiseGenerator.SampleNoise(position);

            position.x += (sample.x * 2f - 1f) * HexMetrics.CellPerturbStrength;
            //position.y += (sample.y * 2f - 1f) * HexMetrics.CellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * HexMetrics.CellPerturbStrength;

            return position;
        }

        private void AddTriangle(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(Perturb(vertexOne));
            Vertices.Add(Perturb(vertexTwo));
            Vertices.Add(Perturb(vertexThree));

            Triangles.Add(vertexIndex);
            Triangles.Add(vertexIndex + 1);
            Triangles.Add(vertexIndex + 2);
        }

        private void AddTriangleUnperturbed(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree) {
            int vertexIndex = Vertices.Count;

		    Vertices.Add(vertexOne);
		    Vertices.Add(vertexTwo);
		    Vertices.Add(vertexThree);

		    Triangles.Add(vertexIndex);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 2);
        }

        private void AddTriangleColor(Color color) {
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
        }

        private void AddTriangleColor(Color colorOne, Color colorTwo, Color colorThree) {
            Colors.Add(colorOne);
            Colors.Add(colorTwo);
            Colors.Add(colorThree);
        }

        private void AddQuad(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree, Vector3 vertexFour) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(Perturb(vertexOne));
		    Vertices.Add(Perturb(vertexTwo));
		    Vertices.Add(Perturb(vertexThree));
		    Vertices.Add(Perturb(vertexFour));

		    Triangles.Add(vertexIndex);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 3);
        }

        private void AddQuadColor(Color colorOne, Color colorTwo, Color colorThree, Color colorFour) {
            Colors.Add(colorOne);
            Colors.Add(colorTwo);
            Colors.Add(colorThree);
            Colors.Add(colorFour);
        }

        private void AddQuadColor(Color colorOne, Color colorTwo) {
            Colors.Add(colorOne);
            Colors.Add(colorOne);

            Colors.Add(colorTwo);
            Colors.Add(colorTwo);
        }

        #endregion

    }

}
