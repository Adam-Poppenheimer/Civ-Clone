using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.HexMap {

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class HexMesh : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        private Mesh ManagedMesh;
        public MeshCollider Collider { get; private set; }

        private HexCellSignals CellSignals;
        private IHexGrid Grid;

        private List<Vector3> Vertices;
        private List<int>     Triangles;
        private List<Color>   Colors;

        private IHexCell LastCellEntered;

        private bool ShouldEmitEnterExitMessages;        

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(HexCellSignals cellSignals, IHexGrid grid) {
            CellSignals = cellSignals;
            Grid = grid;
        }


        #region Unity message methods

        private void Awake() {
            GetComponent<MeshFilter>().mesh = ManagedMesh = new Mesh();
            ManagedMesh.name = "Hex Mesh";
            
            Collider = GetComponent<MeshCollider>();

            Vertices  = new List<Vector3>();
            Triangles = new List<int>();
            Colors    = new List<Color>();
        }

        private void Update() {
            TryEmitEnterExitMessages();
        }

        #endregion

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {

                if(hit.collider == Collider) {
                    var coordinates = HexCoordinates.FromPosition(hit.point);
                    var clickedCell = Grid.GetCellAtCoordinates(coordinates);
                    CellSignals.ClickedSignal.Fire(clickedCell, Input.mousePosition);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            ShouldEmitEnterExitMessages = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            ShouldEmitEnterExitMessages = false;
            if(LastCellEntered != null) {
                EmitExitMessage(LastCellEntered);
                LastCellEntered = null;
            }
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
            Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

            AddTriangle(center, v1, v2);
            AddTriangleColor(cell.Color);

            if(direction <= HexDirection.SE) {
                TriangulateConnection(direction, cell, v1, v2); 
            }                    
        }

        private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2) {
            if(!Grid.HasNeighbor(cell, direction)) {
                return;
            }

            IHexCell neighbor = Grid.GetNeighbor(cell, direction);

            Vector3 bridge = HexMetrics.GetBridge(direction);
            Vector3 v3 = v1 + bridge;
            Vector3 v4 = v2 + bridge;
            v3.y = v4.y = neighbor.Elevation * HexMetrics.ElevationStep;

            var edgeType = HexMetrics.GetEdgeType(cell.Elevation, neighbor.Elevation);
            if(edgeType == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
            }else {
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(cell.Color, neighbor.Color);
            }

            if(direction > HexDirection.E || !Grid.HasNeighbor(cell, direction.Next())) {
                return;
            }

            IHexCell nextNeighbor = Grid.GetNeighbor(cell, direction.Next());

            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.ElevationStep;

            if(cell.Elevation <= neighbor.Elevation) {
                if(cell.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                }else {
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
            }else if(neighbor.Elevation <= nextNeighbor.Elevation) {
                TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
            }else {
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }

            //AddTriangle(v2, v4, v5);
            //AddTriangleColor(cell.Color, neighbor.Color, nextNeighbor.Color);
        }

        private void TriangulateEdgeTerraces(
            Vector3 beginleft, Vector3 beginRight, IHexCell beginCell,
            Vector3 endLeft, Vector3 endRight, IHexCell endCell
        ) {
            Vector3 v3 = HexMetrics.TerraceLerp(beginleft, endLeft, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

            AddQuad(beginleft, beginRight, v3, v4);
            AddQuadColor(beginCell.Color, c2);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c2;

                v3 = HexMetrics.TerraceLerp(beginleft, endLeft, i);
                v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
                c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);

                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2);
            }

            AddQuad(v3, v4, endLeft, endRight);
            AddQuadColor(c2, endCell.Color);
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

            Vector3 boundary = Vector3.Lerp(begin, right, b);
            Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }else {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        private void TriangulateCornerCliffTerraces(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell
        ){
            float b = Mathf.Abs(1f / (leftCell.Elevation - beginCell.Elevation));
            Vector3 boundary = Vector3.Lerp(begin, left, b);
            Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }else {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        private void TriangulateBoundaryTriangle(
            Vector3 begin, IHexCell beginCell,
            Vector3 left, IHexCell leftCell,
            Vector3 boundary, Color boundaryColor
        ) {
            Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            AddTriangle(begin, v2, boundary);
            AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = HexMetrics.TerraceLerp(begin, left, i);
                c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                AddTriangle(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangle(v2, left, boundary);
            AddTriangleColor(c2, leftCell.Color, boundaryColor);
        }

        private void AddTriangle(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree) {
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

            Vertices.Add(vertexOne);
		    Vertices.Add(vertexTwo);
		    Vertices.Add(vertexThree);
		    Vertices.Add(vertexFour);

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

        private IHexCell GetCellUnderPosition(Vector3 position) {
            var pointerRay = Camera.main.ScreenPointToRay(position);

            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {
                if(hit.collider == Collider) {
                    var coordinates = HexCoordinates.FromPosition(hit.point);
                    return Grid.GetCellAtCoordinates(coordinates);
                }
            }

            return null;
        }

        private void TryEmitEnterExitMessages() {
            if( ShouldEmitEnterExitMessages &&
                (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            ){
                var cellUnderMouse = GetCellUnderPosition(Input.mousePosition);
                if(cellUnderMouse != LastCellEntered) {
                    if(LastCellEntered != null) {
                        EmitExitMessage(LastCellEntered);
                    }

                    LastCellEntered = cellUnderMouse;

                    if(LastCellEntered != null) {
                        EmitEnterMessage(LastCellEntered);
                    }
                }
            }
        }

        private void EmitEnterMessage(IHexCell cell) {
            CellSignals.PointerEnterSignal.Fire(LastCellEntered);
        }

        private void EmitExitMessage(IHexCell cell) {
            CellSignals.PointerExitSignal.Fire(cell);
        }

        #endregion

    }

}
