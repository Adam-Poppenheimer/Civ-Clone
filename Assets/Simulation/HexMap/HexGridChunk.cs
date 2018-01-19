using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class HexGridChunk : MonoBehaviour {

        #region instance fields and properties

        private HexCell[] Cells;

        [SerializeField] private HexMesh Terrain;
        [SerializeField] private HexMesh Rivers;
        [SerializeField] private HexMesh Roads;
        [SerializeField] private HexMesh Water;
        [SerializeField] private HexMesh WaterShore;
        [SerializeField] private HexMesh Estuaries;
        [SerializeField] private HexMesh Culture;

        [SerializeField] private HexFeatureManager Features;

        private IHexGrid Grid;

        private INoiseGenerator NoiseGenerator;

        private IRiverCanon RiverCanon;

        private ICityFactory CityFactory;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGrid grid, INoiseGenerator noiseGenerator,
            IRiverCanon riverCanon, ICityFactory cityFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon
        ){
            Grid                = grid;
            NoiseGenerator      = noiseGenerator;
            RiverCanon          = riverCanon;
            CityFactory         = cityFactory;
            CityPossessionCanon = cityPossessionCanon;
            CellPossessionCanon = cellPossessionCanon;
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
            Terrain   .Clear();
            Rivers    .Clear();
            Roads     .Clear();
            Water     .Clear();
            WaterShore.Clear();
            Estuaries .Clear();
            Features  .Clear();
            Culture   .Clear();

            for(int i = 0; i < Cells.Length; ++i) {
                Triangulate(Cells[i]);
            }

            Terrain   .Apply();
            Rivers    .Apply();
            Roads     .Apply();
            Water     .Apply();
            WaterShore.Apply();
            Estuaries .Apply();
            Features  .Apply();
            Culture   .Apply();
        }

        private void Triangulate(IHexCell cell) {
            for(HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; ++direction) {
                Triangulate(direction, cell);
            }

            if( cell.Feature == TerrainFeature.Forest &&
                !RiverCanon.HasRiver(cell) &&
                !CityFactory.AllCities.Exists(city => city.Location == cell)
            ){
                Features.AddFeature(cell.transform.localPosition, cell.Feature);
            }            
        }

        private void Triangulate(HexDirection direction, IHexCell cell) {
            Vector3 center = cell.transform.localPosition;
            EdgeVertices edge = new EdgeVertices(
                center + HexMetrics.GetFirstSolidCorner(direction),
                center + HexMetrics.GetSecondSolidCorner(direction)
            );

            if(RiverCanon.HasRiver(cell)) {
                if(RiverCanon.HasRiverThroughEdge(cell, direction)) {
                    edge.V3.y = cell.StreamBedY;
                    if(RiverCanon.HasRiverBeginOrEnd(cell)) {
                        TriangulateWithRiverBeginOrEnd(direction, cell, center, edge, Terrain);
                    }else {
                        TriangulateWithRiver(direction, cell, center, edge);
                    }                    
                }else {
                    TriangulateAdjacentToRiver(direction, cell, center, edge, Terrain);

                    if(cell.Feature == TerrainFeature.Forest) {
                        Features.AddFeature((center + edge.V1 + edge.V5) * (1f / 3f), cell.Feature);
                    }
                }
            }else {
                TriangulateWithoutRiver(direction, cell, center, edge);

                if(cell.Feature == TerrainFeature.Forest) {
                    Features.AddFeature((center + edge.V1 + edge.V5) * (1f / 3f), cell.Feature);
                }
            }            

            if(direction <= HexDirection.SE) {
                TriangulateConnection(direction, cell, edge, Terrain); 
            }

            if(cell.IsUnderwater) {
                TriangulateWater(direction, cell, center);
            }

            ICity cellOwner = CellPossessionCanon.GetOwnerOfPossession(cell);
            if(cellOwner != null) {
                ICivilization cityOwner = CityPossessionCanon.GetOwnerOfPossession(cellOwner);
                TriangulateCulture(direction, cell, cityOwner);
            }
        }

        private void TriangulateWithoutRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e
        ){
            TriangulateEdgeFan(center, e, cell.Color, Terrain);

            if(cell.HasRoads) {
                Vector2 interpolators = GetRoadInterpolators(direction, cell);

                TriangulateRoad(
                    center,
                    Vector3.Lerp(center, e.V1, interpolators.x),
                    Vector3.Lerp(center, e.V5, interpolators.y),
                    e, cell.HasRoadThroughEdge(direction)
                );
            }
        }

        private void TriangulateWithRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices edge
        ) {
            Vector3 centerLeft, centerRight;
            
            if(RiverCanon.HasRiverThroughEdge(cell, direction.Opposite())){
                centerLeft  = center + HexMetrics.GetFirstSolidCorner (direction.Previous()) * 0.25f;
                centerRight = center + HexMetrics.GetSecondSolidCorner(direction.Next    ()) * 0.25f;

            }else if (RiverCanon.HasRiverThroughEdge(cell, direction.Next())) {
                centerLeft = center;
                centerRight = Vector3.Lerp(center, edge.V5, 2f / 3f);

            }else if(RiverCanon.HasRiverThroughEdge(cell, direction.Previous())) {
                centerLeft = Vector3.Lerp(center, edge.V1, 2f / 3f);
                centerRight = center;

            }else if(RiverCanon.HasRiverThroughEdge(cell, direction.Next2())) {
                centerLeft = center;
                centerRight = center + HexMetrics.GetSolidEdgeMiddle(direction.Next()) * 0.5f * HexMetrics.InnerToOuter;

            }else if(RiverCanon.HasRiverThroughEdge(cell, direction.Previous2())) {
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

            TriangulateEdgeStrip(middle, cell.Color, edge, cell.Color, Terrain);

            Terrain.AddTriangle(centerLeft, middle.V1, middle.V2);
            Terrain.AddTriangleColor(cell.Color);

            Terrain.AddQuad(centerLeft, center, middle.V2, middle.V3);
            Terrain.AddQuadColor(cell.Color);

            Terrain.AddQuad(center, centerRight, middle.V3, middle.V4);
            Terrain.AddQuadColor(cell.Color);

            Terrain.AddTriangle(centerRight, middle.V4, middle.V5);
            Terrain.AddTriangleColor(cell.Color);

            if(!cell.IsUnderwater) {
                bool isReversed = RiverCanon.GetIncomingRiver(cell) == direction;

                TriangulateRiverQuad(centerLeft, centerRight, middle.V2, middle.V4, cell.RiverSurfaceY, 0.4f, isReversed);
                TriangulateRiverQuad(middle.V2,  middle.V4,   edge.V2,   edge.V4,   cell.RiverSurfaceY, 0.6f, isReversed);
            }
        }

        private void TriangulateWithRiverBeginOrEnd(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e,
            HexMesh targetMesh
        ) {
            EdgeVertices middle = new EdgeVertices(
                Vector3.Lerp(center, e.V1, 0.5f),
                Vector3.Lerp(center, e.V5, 0.5f)
            );

            middle.V3.y = e.V3.y;

            TriangulateEdgeStrip(middle, cell.Color, e, cell.Color, targetMesh);
            TriangulateEdgeFan(center, middle, cell.Color, targetMesh);

            if(!cell.IsUnderwater) {
                bool reversed = RiverCanon.HasIncomingRiver(cell);
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
        }

        private void TriangulateAdjacentToRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e,
            HexMesh targetMesh
        ) {
            if(cell.HasRoads) {
                TriangulateRoadsAdjacentToRiver(direction, cell, center, e);
            }

            if(RiverCanon.HasRiverThroughEdge(cell, direction.Next())) {
                if(RiverCanon.HasRiverThroughEdge(cell, direction.Previous())) {
                    center += HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.InnerToOuter * 0.5f;

                }else if(RiverCanon.HasRiverThroughEdge(cell, direction.Previous2())) {
                    center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;

                }
            }else if(
                RiverCanon.HasRiverThroughEdge(cell, direction.Previous()) &&
                RiverCanon.HasRiverThroughEdge(cell, direction.Next2())
            ) {
                center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
            }

            EdgeVertices middle = new EdgeVertices(
                Vector3.Lerp(center, e.V1, 0.5f),
                Vector3.Lerp(center, e.V5, 0.5f)
            );

            TriangulateEdgeStrip(middle, cell.Color, e, cell.Color, targetMesh);
            TriangulateEdgeFan(center, middle, cell.Color, targetMesh);
        }

        private void TriangulateConnection(
            HexDirection direction, IHexCell cell, EdgeVertices edgeOne, 
            HexMesh targetMesh
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

            if(RiverCanon.HasRiverThroughEdge(cell, direction)) {
                edgeTwo.V3.y = neighbor.StreamBedY;

                if(!cell.IsUnderwater) {
                    if(!neighbor.IsUnderwater) {
                        TriangulateRiverQuad(
                            edgeOne.V2, edgeOne.V4, edgeTwo.V2, edgeTwo.V4,
                            cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                            RiverCanon.HasIncomingRiver(cell) && RiverCanon.GetIncomingRiver(cell) == direction
                        );
                    }else if(cell.Elevation > neighbor.WaterLevel){
                        TriangulateWaterfallInWater(
                            edgeOne.V2, edgeOne.V4, edgeTwo.V2, edgeTwo.V4,
                            cell.RiverSurfaceY, neighbor.RiverSurfaceY,
                            neighbor.WaterSurfaceY
                        );
                    }
                }else if(!neighbor.IsUnderwater && neighbor.Elevation > cell.WaterLevel) {
                    TriangulateWaterfallInWater(
                        edgeTwo.V4, edgeTwo.V2, edgeOne.V4, edgeOne.V2,
                        neighbor.RiverSurfaceY, cell.RiverSurfaceY,
                        cell.WaterSurfaceY
                    );
                }
            }

            var edgeType = HexMetrics.GetEdgeType(cell.Elevation, neighbor.Elevation);
            if(edgeType == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(
                    edgeOne, cell, edgeTwo, neighbor, targetMesh, cell.HasRoadThroughEdge(direction)
                );
            }else {
                TriangulateEdgeStrip(
                    edgeOne, cell.Color, edgeTwo, neighbor.Color,
                    targetMesh, cell.HasRoadThroughEdge(direction)
                );
            }

            if(direction > HexDirection.E || !Grid.HasNeighbor(cell, direction.Next())) {
                return;
            }

            IHexCell nextNeighbor = Grid.GetNeighbor(cell, direction.Next());

            Vector3 v5 = edgeOne.V5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.transform.localPosition.y;

            if(cell.Elevation <= neighbor.Elevation) {
                if(cell.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCorner(edgeOne.V5, cell, edgeTwo.V5, neighbor, v5, nextNeighbor, targetMesh);
                }else {
                    TriangulateCorner(v5, nextNeighbor, edgeOne.V5, cell, edgeTwo.V5, neighbor, targetMesh);
                }
            }else if(neighbor.Elevation <= nextNeighbor.Elevation) {
                TriangulateCorner(edgeTwo.V5, neighbor, v5, nextNeighbor, edgeOne.V5, cell, targetMesh);
            }else {
                TriangulateCorner(v5, nextNeighbor, edgeOne.V5, cell, edgeTwo.V5, neighbor, targetMesh);
            }

        }

        private void TriangulateEdgeTerraces(
            EdgeVertices begin, IHexCell beginCell,
            EdgeVertices end, IHexCell endCell,
            HexMesh targetMesh, bool hasRoad
        ) {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

            TriangulateEdgeStrip(begin, beginCell.Color, e2, c2, targetMesh, hasRoad);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices e1 = e2;
                Color c1 = c2;

                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);

                TriangulateEdgeStrip(e1, c1, e2, c2, targetMesh, hasRoad);
            }

            TriangulateEdgeStrip(e2, c2, end, endCell.Color, targetMesh, hasRoad);
        }

        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color, HexMesh targetMesh) {
            targetMesh.AddTriangle(center, edge.V1, edge.V2);
            targetMesh.AddTriangleColor(color);

            targetMesh.AddTriangle(center, edge.V2, edge.V3);
            targetMesh.AddTriangleColor(color);

            targetMesh.AddTriangle(center, edge.V3, edge.V4);
            targetMesh.AddTriangleColor(color);

            targetMesh.AddTriangle(center, edge.V4, edge.V5);
            targetMesh.AddTriangleColor(color);
        }

        private void TriangulateEdgeStrip(
            EdgeVertices e1, Color c1,
            EdgeVertices e2, Color c2,
            HexMesh targetMesh,
            bool hasRoad = false
        ) {
            targetMesh.AddQuad(e1.V1, e1.V2, e2.V1, e2.V2);
            targetMesh.AddQuadColor(c1, c2);

		    targetMesh.AddQuad(e1.V2, e1.V3, e2.V2, e2.V3);
		    targetMesh.AddQuadColor(c1, c2);

            targetMesh.AddQuad(e1.V3, e1.V4, e2.V3, e2.V4);
		    targetMesh.AddQuadColor(c1, c2);

		    targetMesh.AddQuad(e1.V4, e1.V5, e2.V4, e2.V5);
		    targetMesh.AddQuadColor(c1, c2);

            if(hasRoad) {
                TriangulateRoadSegment(e1.V2, e1.V3, e1.V4, e2.V2, e2.V3, e2.V4);
            }
        }

        private void TriangulateCorner(
            Vector3 bottom, IHexCell bottomCell,
            Vector3 left,   IHexCell leftCell,
            Vector3 right,  IHexCell rightCell,
            HexMesh targetMesh
        ) {
            HexEdgeType leftEdgeType  = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if(leftEdgeType == HexEdgeType.Slope) {

                if(rightEdgeType == HexEdgeType.Slope) {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell, targetMesh);

                }else if(rightEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell, targetMesh);
                }else {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell, targetMesh);
                }

            }else if(rightEdgeType == HexEdgeType.Slope) {

                if(leftEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell, targetMesh);
                }else {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell, targetMesh);
                }

            }else if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {

                if(leftCell.Elevation < rightCell.Elevation) {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell, targetMesh);
                }else {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell, targetMesh);
                }

            }else {
                targetMesh.AddTriangle(bottom, left, right);
                targetMesh.AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
            }            
        }

        private void TriangulateCornerTerraces(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell,
            HexMesh targetMesh
        ){
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);

            Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
            Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

            targetMesh.AddTriangle(begin, v3, v4);
            targetMesh.AddTriangleColor(beginCell.Color, c3, c4);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;

                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);

                targetMesh.AddQuad(v1, v2, v3, v4);
                targetMesh.AddQuadColor(c1, c2, c3, c4);
            }

            targetMesh.AddQuad(v3, v4, left, right);
            targetMesh.AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
        }

        private void TriangulateCornerTerracesCliff(
            Vector3 begin, IHexCell beginCell,
            Vector3 left, IHexCell leftCell,
            Vector3 right, IHexCell rightCell,
            HexMesh targetMesh
        ){
            float b = Mathf.Abs(1f / (rightCell.Elevation - beginCell.Elevation));

            Vector3 boundary = Vector3.Lerp(NoiseGenerator.Perturb(begin), NoiseGenerator.Perturb(right), b);
            Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor, targetMesh);

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor, targetMesh);
            }else {
                targetMesh.AddTriangleUnperturbed(NoiseGenerator.Perturb(left), NoiseGenerator.Perturb(right), boundary);
                targetMesh.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        private void TriangulateCornerCliffTerraces(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell,
            HexMesh targetMesh
        ){
            float b = Mathf.Abs(1f / (leftCell.Elevation - beginCell.Elevation));
            Vector3 boundary = Vector3.Lerp(NoiseGenerator.Perturb(begin), NoiseGenerator.Perturb(left), b);
            Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor, targetMesh);

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor, targetMesh);
            }else {
                targetMesh.AddTriangleUnperturbed(NoiseGenerator.Perturb(left), NoiseGenerator.Perturb(right), boundary);
                targetMesh.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        private void TriangulateBoundaryTriangle(
            Vector3 begin, IHexCell beginCell,
            Vector3 left, IHexCell leftCell,
            Vector3 boundary, Color boundaryColor,
            HexMesh targetMesh
        ) {
            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            targetMesh.AddTriangleUnperturbed(NoiseGenerator.Perturb(begin), v2, boundary);
            targetMesh.AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                targetMesh.AddTriangleUnperturbed(v1, v2, boundary);
                targetMesh.AddTriangleColor(c1, c2, boundaryColor);
            }

            targetMesh.AddTriangleUnperturbed(v2, NoiseGenerator.Perturb(left), boundary);
            targetMesh.AddTriangleColor(c2, leftCell.Color, boundaryColor);
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

        private void TriangulateRoadSegment(
            Vector3 v1, Vector3 v2, Vector3 v3,
            Vector3 v4, Vector3 v5, Vector3 v6
        ) {
            Roads.AddQuad(v1, v2, v4, v5);
            Roads.AddQuad(v2, v3, v5, v6);
            Roads.AddQuadUV(0f, 1f, 0f, 0f);
            Roads.AddQuadUV(1f, 0f, 0f, 0f);
        }

        private void TriangulateRoad(
            Vector3 center, Vector3 middleLeft, Vector3 middleRight, EdgeVertices e,
            bool hasRoadThroughCellEdge
        ) {
            if(hasRoadThroughCellEdge) {
                Vector3 middleCenter = Vector3.Lerp(middleLeft, middleRight, 0.5f);

                TriangulateRoadSegment(middleLeft, middleCenter, middleRight, e.V2, e.V3, e.V4);

                Roads.AddTriangle(center, middleLeft, middleCenter);
                Roads.AddTriangle(center, middleCenter, middleRight);
                Roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f)
                );
                Roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f)
                );
            }else {
                TriangulateRoadEdge(center, middleLeft, middleRight);
            }            
        }

        private void TriangulateRoadEdge(Vector3 center, Vector3 middleLeft, Vector3 middleRight) {
            Roads.AddTriangle(center, middleLeft, middleRight);
            Roads.AddTriangleUV(
                new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );
        }

        private void TriangulateRoadsAdjacentToRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e
        ){
            bool hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
            bool previousHasRiver   = RiverCanon.HasRiverThroughEdge(cell, direction.Previous());
            bool nextHasRiver       = RiverCanon.HasRiverThroughEdge(cell, direction.Next());

            Vector2 interpolators = GetRoadInterpolators(direction, cell);
            Vector3 roadCenter = center;

            if(RiverCanon.HasRiverBeginOrEnd(cell)) {
                roadCenter += HexMetrics.GetSolidEdgeMiddle(
                    RiverCanon.GetRiverBeginOrEndDirection(cell).Opposite()
                ) * (1f / 3f);

            }else if(RiverCanon.GetIncomingRiver(cell) == RiverCanon.GetOutgoingRiver(cell).Opposite()) {
                Vector3 corner;
                if(previousHasRiver) {
                    if( !hasRoadThroughEdge &&
                        !cell.HasRoadThroughEdge(direction.Next())
                    ) {
                        return;
                    }
                    corner = HexMetrics.GetSecondSolidCorner(direction);
                }else {
                    if( !hasRoadThroughEdge &&
                        !cell.HasRoadThroughEdge(direction.Previous())
                    ) {
                        return;
                    }
                    corner = HexMetrics.GetFirstSolidCorner(direction);
                }
                roadCenter += corner * 0.5f;
                center += corner * 0.25f;

            }else if(RiverCanon.GetIncomingRiver(cell) == RiverCanon.GetOutgoingRiver(cell).Previous()) {
                roadCenter -= HexMetrics.GetSecondCorner(RiverCanon.GetOutgoingRiver(cell)) * 0.2f;

            }else if(RiverCanon.GetIncomingRiver(cell) == RiverCanon.GetOutgoingRiver(cell).Next()) {
                roadCenter -= HexMetrics.GetFirstCorner(RiverCanon.GetIncomingRiver(cell)) * 0.2f;

            }else if(previousHasRiver && nextHasRiver) {
                if(!hasRoadThroughEdge) {
                    return;
                }
                Vector3 offset = HexMetrics.GetSolidEdgeMiddle(direction) *
                    HexMetrics.InnerToOuter;

                roadCenter += offset * 0.7f;
                center     += offset * 0.5f;

            }else {
                HexDirection middle;
                if(previousHasRiver) {
                    middle = direction.Next();
                }else if(nextHasRiver) {
                    middle = direction.Previous();
                }else {
                    middle = direction;
                }

                if( !cell.HasRoadThroughEdge(middle) &&
                    !cell.HasRoadThroughEdge(middle.Previous()) &&
                    !cell.HasRoadThroughEdge(middle.Next())
                ) {
                    return;
                }

                roadCenter += HexMetrics.GetSolidEdgeMiddle(middle) * 0.25f;
            }

            Vector3 middleLeft  = Vector3.Lerp(roadCenter, e.V1, interpolators.x);
            Vector3 middleRight = Vector3.Lerp(roadCenter, e.V5, interpolators.y);
            TriangulateRoad(roadCenter, middleLeft, middleRight, e, hasRoadThroughEdge);

            if(previousHasRiver) {
                TriangulateRoadEdge(roadCenter, center, middleLeft);
            }
            if(nextHasRiver) {
                TriangulateRoadEdge(roadCenter, middleRight, center);
            }
        }

        private Vector2 GetRoadInterpolators(HexDirection direction, IHexCell cell) {
            Vector2 interpolators;
            if(cell.HasRoadThroughEdge(direction)) {
                interpolators.x = interpolators.y = 0.5f;

            }else {
                interpolators.x = cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
                interpolators.y = cell.HasRoadThroughEdge(direction.Next())     ? 0.5f : 0.25f;
            }
            return interpolators;
        }

        private void TriangulateWater(
            HexDirection direction, IHexCell cell, Vector3 center
        ) {
            center.y = cell.WaterSurfaceY;

            IHexCell neighbor = Grid.GetNeighbor(cell, direction);
            if(neighbor != null && !neighbor.IsUnderwater) {
                TriangulateWaterShore(direction, cell, neighbor, center);
            }else {
                TriangulateOpenWater(direction, cell, neighbor, center);
            }

            
        }

        private void TriangulateWaterShore(
            HexDirection direction, IHexCell cell, IHexCell neighbor, Vector3 center
        ) {
            EdgeVertices edgeOne = new EdgeVertices(
                center + HexMetrics.GetFirstWaterCorner(direction),
                center + HexMetrics.GetSecondWaterCorner(direction)
            );

            Water.AddTriangle(center, edgeOne.V1, edgeOne.V2);
            Water.AddTriangle(center, edgeOne.V2, edgeOne.V3);
            Water.AddTriangle(center, edgeOne.V3, edgeOne.V4);
            Water.AddTriangle(center, edgeOne.V4, edgeOne.V5);

            Vector3 centerTwo = neighbor.transform.localPosition;
            centerTwo.y = center.y;

            EdgeVertices edgeTwo = new EdgeVertices(
                centerTwo + HexMetrics.GetSecondSolidCorner(direction.Opposite()),
                centerTwo + HexMetrics.GetFirstSolidCorner (direction.Opposite())
            );

            if(RiverCanon.HasRiverThroughEdge(cell, direction)) {
                TriangulateEstuary(edgeOne, edgeTwo, RiverCanon.GetIncomingRiver(cell) == direction);
            }else {
                WaterShore.AddQuad(edgeOne.V1, edgeOne.V2, edgeTwo.V1, edgeTwo.V2);
                WaterShore.AddQuad(edgeOne.V2, edgeOne.V3, edgeTwo.V2, edgeTwo.V3);
                WaterShore.AddQuad(edgeOne.V3, edgeOne.V4, edgeTwo.V3, edgeTwo.V4);
                WaterShore.AddQuad(edgeOne.V4, edgeOne.V5, edgeTwo.V4, edgeTwo.V5);

                WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
                WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
                WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
                WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
            }

            IHexCell nextNeighbor = Grid.GetNeighbor(cell, direction.Next());
            if(nextNeighbor != null) {
                Vector3 v3 = nextNeighbor.transform.localPosition + (
                    nextNeighbor.IsUnderwater ? 
                    HexMetrics.GetFirstWaterCorner(direction.Previous()) :
                    HexMetrics.GetFirstSolidCorner(direction.Previous())
                );
                v3.y = center.y;

                WaterShore.AddTriangle(edgeOne.V5, edgeTwo.V5, v3);

                WaterShore.AddTriangleUV(
                    new Vector2(0f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f)
                );
            }
        }

        private void TriangulateOpenWater(
            HexDirection direction, IHexCell cell, IHexCell neighbor, Vector3 center
        ) {
            Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(direction);
            Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(direction);

            Water.AddTriangle(center, c1, c2);

            if(direction <= HexDirection.SE && neighbor != null) {
                Vector3 bridge = HexMetrics.GetWaterBridge(direction);
                Vector3 e1 = c1 + bridge;
                Vector3 e2 = c2 + bridge;

                Water.AddQuad(c1, c2, e1, e2);

                if(direction <= HexDirection.E && Grid.HasNeighbor(cell, direction.Next())) {
                    var nextNeighbor = Grid.GetNeighbor(cell, direction.Next());
                    if(!nextNeighbor.IsUnderwater) {
                        return;
                    }

                    Water.AddTriangle(c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next()));
                }
            }
        }

        private void TriangulateWaterfallInWater(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2, float waterY
        ) {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;

            v1 = NoiseGenerator.Perturb(v1);
            v2 = NoiseGenerator.Perturb(v2);
            v3 = NoiseGenerator.Perturb(v3);
            v4 = NoiseGenerator.Perturb(v4);

            float t = (waterY - y2) / (y1 - y2);
            v3 = Vector3.Lerp(v3, v1, t);
            v4 = Vector3.Lerp(v4, v2, t);

            Rivers.AddQuadUnperturbed(v1, v2, v3, v4);
            Rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
        }

        private void TriangulateEstuary(EdgeVertices edgeOne, EdgeVertices edgeTwo, bool incomingRiver) {
            WaterShore.AddTriangle(edgeTwo.V1, edgeOne.V2, edgeOne.V1);
            WaterShore.AddTriangle(edgeTwo.V5, edgeOne.V5, edgeOne.V4);

            WaterShore.AddTriangleUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );
            WaterShore.AddTriangleUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );

            Estuaries.AddQuad(edgeTwo.V1, edgeOne.V2, edgeTwo.V2, edgeOne.V3);
            Estuaries.AddTriangle(edgeOne.V3, edgeTwo.V2, edgeTwo.V4);
            Estuaries.AddQuad(edgeOne.V3, edgeOne.V4, edgeTwo.V4, edgeTwo.V5);

            Estuaries.AddQuadUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 0f)
            );
            Estuaries.AddTriangleUV(
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(1f, 1f)
            );
            Estuaries.AddQuadUV(
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f)
            );

            if(incomingRiver) {
                Estuaries.AddQuadUV2(
                    new Vector2(1.5f, 1f), new Vector2(0.7f, 1.15f),
                    new Vector2(1f, 0.8f), new Vector2(0.5f, 1.1f)
                );
                Estuaries.AddTriangleUV2(
                    new Vector2(0.5f, 1.1f),
                    new Vector2(1f, 0.8f),
                    new Vector2(0f, 0.8f)
                );
                Estuaries.AddQuadUV2(
                    new Vector2(0.5f, 1.1f), new Vector2(0.3f, 1.15f),
                    new Vector2(0f, 0.8f), new Vector2(-0.5f, 1f)
                );
            }else {
                Estuaries.AddQuadUV2(
                    new Vector2(-0.5f, -0.2f), new Vector2(0.3f, -0.35f),
                    new Vector2(0f, 0f), new Vector2(0.5f, -0.3f)
                );
                Estuaries.AddTriangleUV2(
                    new Vector2(0.5f, -0.3f),
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f)
                );
                Estuaries.AddQuadUV2(
                    new Vector2(0.5f, -0.3f), new Vector2(0.7f, -0.35f),
                    new Vector2(1f, 0f), new Vector2(1.5f, -0.2f)
                );
            }
            
        }

        private void TriangulateCulture(HexDirection direction, IHexCell cell, ICivilization owner) {
            Vector3 center = cell.transform.localPosition;
            EdgeVertices edgeOne = new EdgeVertices(
                center + HexMetrics.GetFirstSolidCorner(direction),
                center + HexMetrics.GetSecondSolidCorner(direction)
            );

            TriangulateCultureConnection(direction, cell, edgeOne, owner);
        }

        private void TriangulateCultureConnection(
            HexDirection direction, IHexCell cell, 
            EdgeVertices edgeOne, ICivilization owner
        ){
            if(!Grid.HasNeighbor(cell, direction)) {
                return;
            }

            IHexCell neighbor = Grid.GetNeighbor(cell, direction);
            ICity ownerOfNeighbor = CellPossessionCanon.GetOwnerOfPossession(neighbor);
            if(ownerOfNeighbor != null && CityPossessionCanon.GetOwnerOfPossession(ownerOfNeighbor) == owner) {
                return;
            }

            Vector3 bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.transform.localPosition.y - cell.transform.localPosition.y;
            EdgeVertices edgeTwo = new EdgeVertices(
                edgeOne.V1 + bridge,
                edgeOne.V5 + bridge
            );

            var edgeType = HexMetrics.GetEdgeType(cell.Elevation, neighbor.Elevation);
            if(edgeType == HexEdgeType.Slope) {
                TriangulateCultureTerraces(
                    edgeOne, cell, edgeTwo, neighbor, owner
                );
            }else {
                Culture.AddQuad(edgeOne.V1, edgeOne.V2, edgeTwo.V1, edgeTwo.V2);
		        Culture.AddQuad(edgeOne.V2, edgeOne.V3, edgeTwo.V2, edgeTwo.V3);
		        Culture.AddQuad(edgeOne.V3, edgeOne.V4, edgeTwo.V3, edgeTwo.V4);
		        Culture.AddQuad(edgeOne.V4, edgeOne.V5, edgeTwo.V4, edgeTwo.V5);
                
                Culture.AddQuadColor(owner.Color);
                Culture.AddQuadColor(owner.Color);
                Culture.AddQuadColor(owner.Color);
                Culture.AddQuadColor(owner.Color);

                Culture.AddQuadUV(0f, 0f, 0f, 1f);
                Culture.AddQuadUV(0f, 0f, 0f, 1f);
                Culture.AddQuadUV(0f, 0f, 0f, 1f);
                Culture.AddQuadUV(0f, 0f, 0f, 1f);
            }

            IHexCell nextNeighbor = Grid.GetNeighbor(cell, direction.Next());
            if(nextNeighbor == null) {
                return;
            }

            if(direction > HexDirection.E) {
                /*var nextNeighborCity = CellPossessionCanon.GetOwnerOfPossession(nextNeighbor);
                if(nextNeighborCity != null) {
                    return;
                }*/
            }

            Vector3 v5 = edgeOne.V5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.transform.localPosition.y;

            if(cell.Elevation <= neighbor.Elevation) {
                if(cell.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCultureCorner(edgeOne.V5, cell, edgeTwo.V5, neighbor, v5, nextNeighbor, owner);
                }else {
                    TriangulateCultureCorner(v5, nextNeighbor, edgeOne.V5, cell, edgeTwo.V5, neighbor, owner);
                }
            }else if(neighbor.Elevation <= nextNeighbor.Elevation) {
                TriangulateCultureCorner(edgeTwo.V5, neighbor, v5, nextNeighbor, edgeOne.V5, cell, owner);
            }else {
                TriangulateCultureCorner(v5, nextNeighbor, edgeOne.V5, cell, edgeTwo.V5, neighbor, owner);
            }
        }

        private void TriangulateCultureTerraces(
            EdgeVertices begin, IHexCell beginCell,
            EdgeVertices end, IHexCell endCell,
            ICivilization owner
        ) {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            float uv2 = HexMetrics.TerraceLerp(0f, 1f, 1);

            TriangulateCultureEdgeStrip(begin, e2, owner, 0f, uv2);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices e1 = e2;
                float uv1 = uv2;

                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                uv2 = HexMetrics.TerraceLerp(0f, 1f, i);

                TriangulateCultureEdgeStrip(e1, e2, owner, uv1, uv2);
            }

            TriangulateCultureEdgeStrip(e2, end, owner, uv2, 1f);
        }

        private void TriangulateCultureEdgeStrip(
            EdgeVertices edgeOne, EdgeVertices edgeTwo,
            ICivilization owner, float vMin, float vMax
        ) {
            Culture.AddQuad(edgeOne.V1, edgeOne.V2, edgeTwo.V1, edgeTwo.V2);
            Culture.AddQuadColor(owner.Color);
            Culture.AddQuadUV(0f, 0f, vMin, vMax);

		    Culture.AddQuad(edgeOne.V2, edgeOne.V3, edgeTwo.V2, edgeTwo.V3);
		    Culture.AddQuadColor(owner.Color);
            Culture.AddQuadUV(0f, 0f, vMin, vMax);

            Culture.AddQuad(edgeOne.V3, edgeOne.V4, edgeTwo.V3, edgeTwo.V4);
		    Culture.AddQuadColor(owner.Color);
            Culture.AddQuadUV(0f, 0f, vMin, vMax);

		    Culture.AddQuad(edgeOne.V4, edgeOne.V5, edgeTwo.V4, edgeTwo.V5);
		    Culture.AddQuadColor(owner.Color);
            Culture.AddQuadUV(0f, 0f, vMin, vMax);
        }

        private void TriangulateCultureCorner(
            Vector3 bottom, IHexCell bottomCell,
            Vector3 left,   IHexCell leftCell,
            Vector3 right,  IHexCell rightCell,
            ICivilization owner
        ){
            HexEdgeType leftEdgeType  = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if(leftEdgeType == HexEdgeType.Slope) {

                if(rightEdgeType == HexEdgeType.Slope) {
                    TriangulateCultureCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell, owner);

                }else if(rightEdgeType == HexEdgeType.Flat) {
                    TriangulateCultureCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell, owner);
                }else {
                    TriangulateCultureCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell, owner);
                }

            }else if(rightEdgeType == HexEdgeType.Slope) {

                if(leftEdgeType == HexEdgeType.Flat) {
                    TriangulateCultureCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell, owner);
                }else {
                    TriangulateCultureCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell, owner);
                }

            }else if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {

                if(leftCell.Elevation < rightCell.Elevation) {
                    TriangulateCultureCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell, owner);
                }else {
                    TriangulateCultureCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell, owner);
                }

            }else {
                TriangulateCultureCornerSimple(bottom, bottomCell, left, leftCell, right, rightCell, owner);
            }
        }

        private void TriangulateCultureCornerTerraces(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell,
            ICivilization owner
        ){
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);

            float uvMin, leftUVMax, rightUVMax;
            CalculateCultureUVs
                (beginCell, leftCell, rightCell, owner,
                out uvMin, out leftUVMax, out rightUVMax
            );

            float uv3 = HexMetrics.TerraceLerp(uvMin, leftUVMax, 1);
            float uv4 = HexMetrics.TerraceLerp(uvMin, rightUVMax, 1);

            Culture.AddTriangle(begin, v3, v4);
            Culture.AddTriangleColor(owner.Color);
            Culture.AddTriangleUV(
                new Vector2(0f, uvMin),
                new Vector2(0f, uv3),
                new Vector2(0f, uv4)
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                float uv1 = uv3;
                float uv2 = uv4;

                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                uv3 = HexMetrics.TerraceLerp(uvMin, leftUVMax, i);
                uv4 = HexMetrics.TerraceLerp(uvMin, rightUVMax, i);

                Culture.AddQuad(v1, v2, v3, v4);
                Culture.AddQuadColor(owner.Color);
                Culture.AddQuadUV(
                    new Vector2(0f, uv1), new Vector2(0f, uv2),
                    new Vector2(0f, uv3), new Vector2(0f, uv4)
                );
            }

            Culture.AddQuad(v3, v4, left, right);
            Culture.AddQuadColor(owner.Color);
            Culture.AddQuadUV(
                new Vector2(0f, uv3), new Vector2(0f, uv4),
                new Vector2(0f, leftUVMax), new Vector2(0f, rightUVMax)
            );
        }

        private bool IsOwnedByCivilization(IHexCell cell, ICivilization civilization) {
            ICity cityOwner = CellPossessionCanon.GetOwnerOfPossession(cell);
            return cityOwner != null && CityPossessionCanon.GetOwnerOfPossession(cityOwner) == civilization;
        }

        private void TriangulateCultureCornerTerracesCliff(
            Vector3 begin, IHexCell beginCell,
            Vector3 left, IHexCell leftCell,
            Vector3 right, IHexCell rightCell,
            ICivilization owner
        ){
            float uvMin, leftUVMax, rightUVMax;
            CalculateCultureUVs
                (beginCell, leftCell, rightCell, owner,
                out uvMin, out leftUVMax, out rightUVMax
            );

            float b = Mathf.Abs(1f / (rightCell.Elevation - beginCell.Elevation));
            float boundaryUV = Mathf.Lerp(uvMin, rightUVMax, b);

            Vector3 boundary = Vector3.Lerp(NoiseGenerator.Perturb(begin), NoiseGenerator.Perturb(right), b);

            TriangulateCultureBoundaryTriangle(
                begin, beginCell, uvMin,
                left, leftCell, leftUVMax,
                boundary, boundaryUV,
                owner
            );

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateCultureBoundaryTriangle(
                    left, leftCell, leftUVMax,
                    right, rightCell, rightUVMax,
                    boundary, boundaryUV,
                    owner
                );
            }else {
                Culture.AddTriangleUnperturbed(NoiseGenerator.Perturb(left), NoiseGenerator.Perturb(right), boundary);
                Culture.AddTriangleColor(owner.Color);
                Culture.AddTriangleUV(
                    new Vector2(0f, leftUVMax),
                    new Vector2(0f, rightUVMax),
                    new Vector2(0f, boundaryUV)
                );
            }
        }

        private void TriangulateCultureCornerCliffTerraces(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell,
            ICivilization owner
        ){
            float uvMin, leftUVMax, rightUVMax;
            CalculateCultureUVs
                (beginCell, leftCell, rightCell, owner,
                out uvMin, out leftUVMax, out rightUVMax
            );

            float b = Mathf.Abs(1f / (leftCell.Elevation - beginCell.Elevation));
            float boundaryUV = Mathf.Lerp(uvMin, leftUVMax, b);

            Vector3 boundary = Vector3.Lerp(NoiseGenerator.Perturb(begin), NoiseGenerator.Perturb(left), b);

            TriangulateCultureBoundaryTriangle(
                right, rightCell, rightUVMax,
                begin, beginCell, uvMin,
                boundary, boundaryUV,
                owner
            );

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateCultureBoundaryTriangle(
                    left, leftCell, leftUVMax,
                    right, rightCell, rightUVMax,
                    boundary, boundaryUV,
                    owner
                );
            }else {
                Culture.AddTriangleUnperturbed(NoiseGenerator.Perturb(left), NoiseGenerator.Perturb(right), boundary);
                Culture.AddTriangleColor(owner.Color);
                Culture.AddTriangleUV(
                    new Vector2(0f, leftUVMax),
                    new Vector2(0f, rightUVMax),
                    new Vector2(0f, boundaryUV)
                );
            }
        }

        private void TriangulateCultureBoundaryTriangle(
            Vector3 begin, IHexCell beginCell, float beginUV,
            Vector3 left, IHexCell leftCell, float leftUV,
            Vector3 boundary, float boundaryUV,
            ICivilization owner
        ) {
            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            float uv2 = HexMetrics.TerraceLerp(beginUV, leftUV, 1);

            Culture.AddTriangleUnperturbed(NoiseGenerator.Perturb(begin), v2, boundary);
            Culture.AddTriangleColor(owner.Color);
            Culture.AddTriangleUV(
                new Vector2(0f, beginUV),
                new Vector2(0f, uv2),
                new Vector2(0f, boundaryUV)
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                float uv1 = uv2;

                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                uv2 = HexMetrics.TerraceLerp(beginUV, leftUV, i);

                Culture.AddTriangleUnperturbed(v1, v2, boundary);
                Culture.AddTriangleColor(owner.Color);
                Culture.AddTriangleUV(
                    new Vector2(0f, uv1),
                    new Vector2(0f, uv2),
                    new Vector2(0f, boundaryUV)
                );
            }

            Culture.AddTriangleUnperturbed(v2, NoiseGenerator.Perturb(left), boundary);
            Culture.AddTriangleColor(owner.Color);
            Culture.AddTriangleUV(
                new Vector2(0f, uv2),
                new Vector2(0f, leftUV),
                new Vector2(0f, boundaryUV)
            );
        }

        private void TriangulateCultureCornerSimple(
            Vector3 begin, IHexCell beginCell,
            Vector3 left,  IHexCell leftCell,
            Vector3 right, IHexCell rightCell,
            ICivilization owner
        ) {
            Culture.AddTriangle(begin, left, right);
            Culture.AddTriangleColor(owner.Color);

            float uvMin, leftUVMax, rightUVMax;
            CalculateCultureUVs
                (beginCell, leftCell, rightCell, owner,
                out uvMin, out leftUVMax, out rightUVMax
            );

            Culture.AddTriangleUV(
                new Vector2(0f, uvMin), new Vector2(0f, leftUVMax), new Vector2(0f, rightUVMax)
            );
        }

        private void CalculateCultureUVs(
            IHexCell beginCell, IHexCell leftCell,
            IHexCell rightCell, ICivilization owner,
            out float uvMin, out float leftUVMax, out float rightUVMax
        ){
            if(IsOwnedByCivilization(beginCell, owner)) {
                if(IsOwnedByCivilization(leftCell, owner)) {
                    //Bottom owned, left owned, right unowned
                    uvMin = leftUVMax = 0f;
                    rightUVMax = 1f;

                }else if(IsOwnedByCivilization(rightCell, owner)) {
                    //Bottom owned, left unowned, right owned
                    uvMin = rightUVMax = 0f;
                    leftUVMax = 1f;

                }else {
                    //Bottom owned, left unowned, right unowned
                    uvMin = 0f;
                    leftUVMax = rightUVMax = 1f;

                }
            }else if(IsOwnedByCivilization(leftCell, owner)){
                if(IsOwnedByCivilization(rightCell, owner)) {
                    //bottom unowned, left owned, right owned
                    uvMin = 1f;
                    leftUVMax = rightUVMax = 0f;

                }else {
                    //bottom unowned, left owned, right unowned
                    uvMin = rightUVMax = 1f;
                    leftUVMax = 0f;
                }
            }else {
                //bottom unowned, left unowned, right owned
                uvMin = leftUVMax = 1f;
                rightUVMax = 0f;
            }
        }

        #endregion

    }

}
