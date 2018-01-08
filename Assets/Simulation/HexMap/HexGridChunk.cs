using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexGridChunk : MonoBehaviour {

        #region instance fields and properties

        private HexCell[] Cells;

        [SerializeField] private HexMesh Terrain;
        [SerializeField] private HexMesh Rivers;

        private IHexGrid Grid;

        private INoiseGenerator NoiseGenerator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IHexGrid grid, INoiseGenerator noiseGenerator) {
            Grid = grid;
            NoiseGenerator = noiseGenerator;
        }

        #region Unity messages

        private void Awake() {
            Cells = new HexCell[HexMetrics.ChunkSizeX * HexMetrics.ChunkSizeZ];
        }

        private void LateUpdate() {
            Triangulate();
            enabled = false;
        }

        #endregion

        public void AddCell(int index, HexCell cell) {
            Cells[index] = cell;
            cell.transform.SetParent(transform, false);
        }

        public void Refresh() {
            enabled = true;
        }

        public void Triangulate() {
            Terrain.Clear();
            Rivers .Clear();

            for(int i = 0; i < Cells.Length; ++i) {
                Triangulate(Cells[i]);
            }

            Terrain.Apply();
            Rivers .Apply();
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

            if(cell.HasRiver) {
                if(cell.HasRiverThroughEdge(direction)) {
                    edge.V3.y = cell.StreamBedY;
                    if(cell.HasRiverBeginOrEnd) {
                        TriangulateWithRiverBeginOrEnd(direction, cell, center, edge);
                    }else {
                        TriangulateWithRiver(direction, cell, center, edge);
                    }                    
                }else {
                    TriangulateAdjacentToRiver(direction, cell, center, edge);
                }
            }else {
                TriangulateEdgeFan(center, edge, cell.Color);
            }            

            if(direction <= HexDirection.SE) {
                TriangulateConnection(direction, cell, edge); 
            }                    
        }

        private void TriangulateWithRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices edge
        ) {
            Vector3 centerLeft, centerRight;
            
            if(cell.HasRiverThroughEdge(direction.Opposite())){
                centerLeft  = center + HexMetrics.GetFirstSolidCorner (direction.Previous()) * 0.25f;
                centerRight = center + HexMetrics.GetSecondSolidCorner(direction.Next    ()) * 0.25f;

            }else if (cell.HasRiverThroughEdge(direction.Next())) {
                centerLeft = center;
                centerRight = Vector3.Lerp(center, edge.V5, 2f / 3f);

            }else if(cell.HasRiverThroughEdge(direction.Previous())) {
                centerLeft = Vector3.Lerp(center, edge.V1, 2f / 3f);
                centerRight = center;

            }else if(cell.HasRiverThroughEdge(direction.Next2())) {
                centerLeft = center;
                centerRight = center + HexMetrics.GetSolidEdgeMiddle(direction.Next()) * 0.5f * HexMetrics.InnerToOuter;

            }else if(cell.HasRiverThroughEdge(direction.Previous2())) {
                centerLeft = center + HexMetrics.GetSolidEdgeMiddle(direction.Previous()) * 0.5f * HexMetrics.InnerToOuter;
                centerRight = center;

            } else {
                centerLeft = centerRight = center;
            }

            center = Vector3.Lerp(centerLeft, centerRight, 0.5f);

            EdgeVertices middle = new EdgeVertices(
                Vector3.Lerp(centerLeft, edge.V1, 0.5f),
                Vector3.Lerp(centerRight, edge.V5, 0.5f),
                1f / 6f
            );

            middle.V3.y = center.y = edge.V3.y;

            TriangulateEdgeStrip(middle, cell.Color, edge, cell.Color);

            Terrain.AddTriangle(centerLeft, middle.V1, middle.V2);
            Terrain.AddTriangleColor(cell.Color);

            Terrain.AddQuad(centerLeft, center, middle.V2, middle.V3);
            Terrain.AddQuadColor(cell.Color);

            Terrain.AddQuad(center, centerRight, middle.V3, middle.V4);
            Terrain.AddQuadColor(cell.Color);

            Terrain.AddTriangle(centerRight, middle.V4, middle.V5);
            Terrain.AddTriangleColor(cell.Color);

            bool isReversed = cell.IncomingRiver == direction;

            TriangulateRiverQuad(centerLeft, centerRight, middle.V2, middle.V4, cell.RiverSurfaceY, 0.4f, isReversed);
            TriangulateRiverQuad(middle.V2,  middle.V4,   edge.V2,   edge.V4,   cell.RiverSurfaceY, 0.6f, isReversed);
        }

        private void TriangulateWithRiverBeginOrEnd(
            HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e
        ) {
            EdgeVertices middle = new EdgeVertices(
                Vector3.Lerp(center, e.V1, 0.5f),
                Vector3.Lerp(center, e.V5, 0.5f)
            );

            middle.V3.y = e.V3.y;

            TriangulateEdgeStrip(middle, cell.Color, e, cell.Color);
            TriangulateEdgeFan(center, middle, cell.Color);

            bool reversed = cell.HasIncomingRiver;
            TriangulateRiverQuad(middle.V2, middle.V4, e.V2, e.V4, cell.RiverSurfaceY, 0.6f, reversed);

            center.y = middle.V2.y = middle.V4.y = cell.RiverSurfaceY;
            Rivers.AddTriangle(center, middle.V2, middle.V4);
            if(reversed) {
                Rivers.AddTriangleUV(
                    new Vector2(0.5f, 0.4f),
                    new Vector2(1f, 0.2f),
                    new Vector2(0f, 0.2f)
                );
            }else {
                Rivers.AddTriangleUV(
                    new Vector2(0.5f, 0.4f),
                    new Vector2(0f, 0.6f),
                    new Vector2(1f, 0.6f)
                );
            }
        }

        private void TriangulateAdjacentToRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e
        ) {
            if(cell.HasRiverThroughEdge(direction.Next())) {
                if(cell.HasRiverThroughEdge(direction.Previous())) {
                    center += HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.InnerToOuter * 0.5f;

                }else if(cell.HasRiverThroughEdge(direction.Previous2())) {
                    center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;

                }
            }else if(
                cell.HasRiverThroughEdge(direction.Previous()) &&
                cell.HasRiverThroughEdge(direction.Next2())
            ) {
                center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
            }

            EdgeVertices middle = new EdgeVertices(
                Vector3.Lerp(center, e.V1, 0.5f),
                Vector3.Lerp(center, e.V5, 0.5f)
            );

            TriangulateEdgeStrip(middle, cell.Color, e, cell.Color);
            TriangulateEdgeFan(center, middle, cell.Color);
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
                edgeOne.V5 + bridge
            );

            if(cell.HasRiverThroughEdge(direction)) {
                edgeTwo.V3.y = neighbor.StreamBedY;
                TriangulateRiverQuad(
                    edgeOne.V2, edgeOne.V4, edgeTwo.V2, edgeTwo.V4,
                    cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                    cell.HasIncomingRiver && cell.IncomingRiver == direction
                );
            }

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

            Vector3 v5 = edgeOne.V5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.transform.localPosition.y;

            if(cell.Elevation <= neighbor.Elevation) {
                if(cell.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCorner(edgeOne.V5, cell, edgeTwo.V5, neighbor, v5, nextNeighbor);
                }else {
                    TriangulateCorner(v5, nextNeighbor, edgeOne.V5, cell, edgeTwo.V5, neighbor);
                }
            }else if(neighbor.Elevation <= nextNeighbor.Elevation) {
                TriangulateCorner(edgeTwo.V5, neighbor, v5, nextNeighbor, edgeOne.V5, cell);
            }else {
                TriangulateCorner(v5, nextNeighbor, edgeOne.V5, cell, edgeTwo.V5, neighbor);
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
            Terrain.AddTriangle(center, edge.V1, edge.V2);
            Terrain.AddTriangleColor(color);

            Terrain.AddTriangle(center, edge.V2, edge.V3);
            Terrain.AddTriangleColor(color);

            Terrain.AddTriangle(center, edge.V3, edge.V4);
            Terrain.AddTriangleColor(color);

            Terrain.AddTriangle(center, edge.V4, edge.V5);
            Terrain.AddTriangleColor(color);
        }

        private void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2) {
            Terrain.AddQuad(e1.V1, e1.V2, e2.V1, e2.V2);
            Terrain.AddQuadColor(c1, c2);

		    Terrain.AddQuad(e1.V2, e1.V3, e2.V2, e2.V3);
		    Terrain.AddQuadColor(c1, c2);

            Terrain.AddQuad(e1.V3, e1.V4, e2.V3, e2.V4);
		    Terrain.AddQuadColor(c1, c2);

		    Terrain.AddQuad(e1.V4, e1.V5, e2.V4, e2.V5);
		    Terrain.AddQuadColor(c1, c2);
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
                Terrain.AddTriangle(bottom, left, right);
                Terrain.AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
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

            Terrain.AddTriangle(begin, v3, v4);
            Terrain.AddTriangleColor(beginCell.Color, c3, c4);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;

                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);

                Terrain.AddQuad(v1, v2, v3, v4);
                Terrain.AddQuadColor(c1, c2, c3, c4);
            }

            Terrain.AddQuad(v3, v4, left, right);
            Terrain.AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
        }

        private void TriangulateCornerTerracesCliff(
            Vector3 begin, IHexCell beginCell,
            Vector3 left, IHexCell leftCell,
            Vector3 right, IHexCell rightCell
        ){
            float b = Mathf.Abs(1f / (rightCell.Elevation - beginCell.Elevation));

            Vector3 boundary = Vector3.Lerp(NoiseGenerator.Perturb(begin), NoiseGenerator.Perturb(right), b);
            Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }else {
                Terrain.AddTriangleUnperturbed(NoiseGenerator.Perturb(left), NoiseGenerator.Perturb(right), boundary);
                Terrain.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        private void TriangulateCornerCliffTerraces(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell
        ){
            float b = Mathf.Abs(1f / (leftCell.Elevation - beginCell.Elevation));
            Vector3 boundary = Vector3.Lerp(NoiseGenerator.Perturb(begin), NoiseGenerator.Perturb(left), b);
            Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }else {
                Terrain.AddTriangleUnperturbed(NoiseGenerator.Perturb(left), NoiseGenerator.Perturb(right), boundary);
                Terrain.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        private void TriangulateBoundaryTriangle(
            Vector3 begin, IHexCell beginCell,
            Vector3 left, IHexCell leftCell,
            Vector3 boundary, Color boundaryColor
        ) {
            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            Terrain.AddTriangleUnperturbed(NoiseGenerator.Perturb(begin), v2, boundary);
            Terrain.AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                Terrain.AddTriangleUnperturbed(v1, v2, boundary);
                Terrain.AddTriangleColor(c1, c2, boundaryColor);
            }

            Terrain.AddTriangleUnperturbed(v2, NoiseGenerator.Perturb(left), boundary);
            Terrain.AddTriangleColor(c2, leftCell.Color, boundaryColor);
        }

        private void TriangulateRiverQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y, float v, bool isReversed
        ) {
            TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, isReversed);
        }

        private void TriangulateRiverQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2, float v, bool isReversed
        ) {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            Rivers.AddQuad(v1, v2, v3, v4);

            if(isReversed) {
                Rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
            }else {
                Rivers.AddQuadUV(0f, 1f,v, v + 0.2f);
            }
        }

        #endregion

    }

}
