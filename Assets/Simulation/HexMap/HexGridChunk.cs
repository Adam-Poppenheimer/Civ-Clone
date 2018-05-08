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

        #region static fields and properties

        private static Color Weights1 = new Color(1f, 0f, 0f);
        private static Color Weights2 = new Color(0f, 1f, 0f);
        private static Color Weights3 = new Color(0f, 0f, 1f);

        #endregion

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

        private IHexGrid                                      Grid;
        private INoiseGenerator                               NoiseGenerator;
        private IRiverCanon                                   RiverCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICity, IHexCell>      CellPossessionCanon;
        private IHexMapConfig                                 Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGrid grid, INoiseGenerator noiseGenerator, IRiverCanon riverCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IHexMapConfig config
        ){
            Grid                = grid;
            NoiseGenerator      = noiseGenerator;
            RiverCanon          = riverCanon;
            CityPossessionCanon = cityPossessionCanon;
            CellPossessionCanon = cellPossessionCanon;
            Config              = config;
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
            if(!RiverCanon.HasRiver(cell)){
                Features.FlagLocationForFeatures(cell.transform.localPosition, cell);
            }

            for(HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; ++direction) {
                Triangulate(direction, cell);
            }
        }

        private void Triangulate(HexDirection direction, IHexCell cell) {
            Vector3 center = cell.transform.localPosition;

            EdgeVertices edge = new EdgeVertices(
                center + HexMetrics.GetFirstOuterSolidCorner(direction),
                center + HexMetrics.GetSecondOuterSolidCorner(direction)
            );

            var cellCenterDelta = cell.PeakY;
            var cellEdgeDelta   = cell.EdgeY;

            center.y = cellCenterDelta;

            edge.V1.y = cellEdgeDelta;
            edge.V2.y = cellEdgeDelta;
            edge.V3.y = cellEdgeDelta;
            edge.V4.y = cellEdgeDelta;
            edge.V5.y = cellEdgeDelta;

            if(RiverCanon.HasRiver(cell)) {
                if(RiverCanon.HasRiverThroughEdge(cell, direction)) {
                    edge.V3.y = cell.StreamBedY;
                    if(RiverCanon.HasRiverBeginOrEnd(cell)) {
                        TriangulateWithRiverBeginOrEnd(direction, cell, center, edge);
                    }else {
                        TriangulateWithRiver(direction, cell, center, edge);
                    }                    
                }else {
                    TriangulateAdjacentToRiver(direction, cell, center, edge);

                    Features.FlagLocationForFeatures((center + edge.V1 + edge.V5) * (1f / 3f), cell);
                }
            }else {
                if(cell.Shape == TerrainShape.Hills) {
                    TriangulateWithoutRiver_Hills(direction, cell, center, edge);
                }else {
                    TriangulateWithoutRiver_NotHills(direction, cell, center, edge);
                }

                Features.FlagLocationForFeatures((center + edge.V1 + edge.V5) * (1f / 3f), cell);
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

        private void TriangulateWithoutRiver_NotHills(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e
        ){
            TriangulateEdgeFan(center, e, cell.Index);

            if(cell.HasRoads) {
                Vector2 interpolators = GetRoadInterpolators(direction, cell);

                var neighbor = Grid.GetNeighbor(cell, direction);

                TriangulateRoad(
                    center,
                    Vector3.Lerp(center, e.V1, interpolators.x),
                    Vector3.Lerp(center, e.V5, interpolators.y),
                    e, ShouldRoadBeRendered(cell, neighbor),
                    cell.Index
                );
            }
        }

        private void TriangulateWithoutRiver_Hills(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices outerEdge
        ) {
            var innerEdge = new EdgeVertices(
                center + HexMetrics.GetFirstInnerSolidCorner(direction),
                center + HexMetrics.GetSecondInnerSolidCorner(direction)
            );

            TriangulateEdgeFan(center, innerEdge, cell.Index, true);
            TriangulateEdgeStrip_Hills(
                innerEdge, Weights1, cell.Index, true,
                outerEdge, Weights1, cell.Index, true
            );
        }

        private void TriangulateWithRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices edge
        ) {
            Vector3 centerLeft, centerRight;
            
            if(RiverCanon.HasRiverThroughEdge(cell, direction.Opposite())){
                centerLeft  = center + HexMetrics.GetFirstOuterSolidCorner (direction.Previous()) * 0.25f;
                centerRight = center + HexMetrics.GetSecondOuterSolidCorner(direction.Next    ()) * 0.25f;

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

            TriangulateEdgeStrip(middle, Weights1, cell.Index, edge, Weights1, cell.Index);

            Terrain.AddTriangle(centerLeft, middle.V1, middle.V2);
            Terrain.AddQuad(centerLeft, center, middle.V2, middle.V3);
            Terrain.AddQuad(center, centerRight, middle.V3, middle.V4);
            Terrain.AddTriangle(centerRight, middle.V4, middle.V5);

            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            Terrain.AddTriangleCellData(indices, Weights1);
            Terrain.AddQuadCellData(indices, Weights1);
            Terrain.AddQuadCellData(indices, Weights1);
            Terrain.AddTriangleCellData(indices, Weights1);

            if(!cell.IsUnderwater) {
                bool isReversed = RiverCanon.GetIncomingRiver(cell) == direction;

                TriangulateRiverQuad(
                    centerLeft, centerRight, middle.V2, middle.V4,
                    cell.RiverSurfaceY, 0.4f, isReversed, indices
                );
                TriangulateRiverQuad(
                    middle.V2,  middle.V4, edge.V2, edge.V4,
                    cell.RiverSurfaceY, 0.6f, isReversed, indices
                );
            }
        }

        private void TriangulateWithRiverBeginOrEnd(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e
        ) {
            EdgeVertices middle = new EdgeVertices(
                Vector3.Lerp(center, e.V1, 0.5f),
                Vector3.Lerp(center, e.V5, 0.5f)
            );

            middle.V3.y = e.V3.y;

            TriangulateEdgeStrip(middle, Weights1, cell.Index, e, Weights1, cell.Index);
            TriangulateEdgeFan(center, middle, cell.Index);

            if(!cell.IsUnderwater) {
                bool reversed = RiverCanon.HasIncomingRiver(cell);

                Vector3 indices;
                indices.x = indices.y = indices.z = cell.Index;

                TriangulateRiverQuad(
                    middle.V2, middle.V4, e.V2, e.V4,
                    cell.RiverSurfaceY, 0.6f, reversed, indices
                );

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

                Rivers.AddTriangleCellData(indices, Weights1);
            }            
        }

        private void TriangulateAdjacentToRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e
        ) {
            if(cell.HasRoads) {
                TriangulateRoadsAdjacentToRiver(direction, cell, center, e);
            }

            if(RiverCanon.HasRiverThroughEdge(cell, direction.Next())) {
                if(RiverCanon.HasRiverThroughEdge(cell, direction.Previous())) {
                    center += HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.InnerToOuter * 0.5f;

                }else if(RiverCanon.HasRiverThroughEdge(cell, direction.Previous2())) {
                    center += HexMetrics.GetFirstOuterSolidCorner(direction) * 0.25f;

                }
            }else if(
                RiverCanon.HasRiverThroughEdge(cell, direction.Previous()) &&
                RiverCanon.HasRiverThroughEdge(cell, direction.Next2())
            ) {
                center += HexMetrics.GetSecondOuterSolidCorner(direction) * 0.25f;
            }

            EdgeVertices middle = new EdgeVertices(
                Vector3.Lerp(center, e.V1, 0.5f),
                Vector3.Lerp(center, e.V5, 0.5f)
            );

            TriangulateEdgeStrip(middle, Weights1, cell.Index, e, Weights1, cell.Index);
            TriangulateEdgeFan(center, middle, cell.Index);
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
            bridge.y = neighbor.EdgeY - cell.EdgeY;
            EdgeVertices edgeTwo = new EdgeVertices(
                edgeOne.V1 + bridge,
                edgeOne.V5 + bridge
            );

            if(RiverCanon.HasRiverThroughEdge(cell, direction)) {
                edgeTwo.V3.y = neighbor.StreamBedY;

                Vector3 indices;
                indices.x = indices.z = cell.Index;
                indices.y = neighbor.Index;

                if(!cell.IsUnderwater) {
                    if(!neighbor.IsUnderwater) {
                        TriangulateRiverQuad(
                            edgeOne.V2, edgeOne.V4, edgeTwo.V2, edgeTwo.V4,
                            cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                            RiverCanon.HasIncomingRiver(cell) && RiverCanon.GetIncomingRiver(cell) == direction,
                            indices
                        );
                    }else if(cell.FoundationElevation > Config.WaterLevel){
                        TriangulateWaterfallInWater(
                            edgeOne.V2, edgeOne.V4, edgeTwo.V2, edgeTwo.V4,
                            cell.RiverSurfaceY, neighbor.RiverSurfaceY,
                            neighbor.WaterSurfaceY, indices
                        );
                    }
                }else if(!neighbor.IsUnderwater && neighbor.FoundationElevation > Config.WaterLevel) {
                    TriangulateWaterfallInWater(
                        edgeTwo.V4, edgeTwo.V2, edgeOne.V4, edgeOne.V2,
                        neighbor.RiverSurfaceY, cell.RiverSurfaceY,
                        cell.WaterSurfaceY, indices
                    );
                }
            }

            var edgeType = HexMetrics.GetEdgeType(cell, neighbor);
            if(edgeType == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(
                    edgeOne, cell, edgeTwo, neighbor, ShouldRoadBeRendered(cell, neighbor)
                );
            }else if(cell.Shape == TerrainShape.Hills || neighbor.Shape == TerrainShape.Hills){
                TriangulateEdgeStrip_Hills(
                    edgeOne, Weights1, cell.Index, cell.Shape == TerrainShape.Hills,
                    edgeTwo, Weights2, neighbor.Index, neighbor.Shape == TerrainShape.Hills,
                    ShouldRoadBeRendered(cell, neighbor)
                );
            }else{
                TriangulateEdgeStrip(
                    edgeOne, Weights1, cell.Index,
                    edgeTwo, Weights2, neighbor.Index,
                    ShouldRoadBeRendered(cell, neighbor)
                );
            }

            if(direction > HexDirection.E || !Grid.HasNeighbor(cell, direction.Next())) {
                return;
            }
            
            IHexCell nextNeighbor = Grid.GetNeighbor(cell, direction.Next());

            Vector3 v5 = edgeOne.V5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.EdgeY;

            if(cell.FoundationElevation <= neighbor.FoundationElevation) {
                if(cell.FoundationElevation <= nextNeighbor.FoundationElevation) {
                    TriangulateCorner(
                        edgeOne.V5, cell,         cell.Shape         == TerrainShape.Hills,
                        edgeTwo.V5, neighbor,     neighbor.Shape     == TerrainShape.Hills,
                        v5,         nextNeighbor, nextNeighbor.Shape == TerrainShape.Hills
                    );
                }else {
                    TriangulateCorner(
                        v5, nextNeighbor,     nextNeighbor.Shape == TerrainShape.Hills,
                        edgeOne.V5, cell,     cell.Shape         == TerrainShape.Hills,
                        edgeTwo.V5, neighbor, neighbor.Shape     == TerrainShape.Hills
                    );
                }
            }else if(neighbor.FoundationElevation <= nextNeighbor.FoundationElevation) {
                TriangulateCorner(
                    edgeTwo.V5, neighbor,     neighbor.Shape     == TerrainShape.Hills,
                    v5,         nextNeighbor, nextNeighbor.Shape == TerrainShape.Hills,
                    edgeOne.V5, cell,         cell.Shape         == TerrainShape.Hills
                );
            }else {
                TriangulateCorner(
                    v5,         nextNeighbor, nextNeighbor.Shape == TerrainShape.Hills,
                    edgeOne.V5, cell,         cell.Shape         == TerrainShape.Hills,
                    edgeTwo.V5, neighbor,     neighbor.Shape     == TerrainShape.Hills
                );
            }
        }

        private void TriangulateEdgeTerraces(
            EdgeVertices begin, IHexCell beginCell,
            EdgeVertices end, IHexCell endCell,
            bool hasRoad
        ) {
            EdgeVertices edgeTwo = EdgeVertices.TerraceLerp(begin, end, 1);
            Color weights2 = HexMetrics.TerraceLerp(Weights1, Weights2, 1);
            float i1 = beginCell.Index;
            float i2 = endCell.Index;

            TriangulateEdgeStrip_Hills(
                begin,   Weights1, i1, beginCell.Shape == TerrainShape.Hills,
                edgeTwo, weights2, i2, false,
                hasRoad
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices edgeOne = edgeTwo;
                Color weights1 = weights2;

                edgeTwo = EdgeVertices.TerraceLerp(begin, end, i);
                weights2 = HexMetrics.TerraceLerp(Weights1, Weights2, i);

                TriangulateEdgeStrip_Hills(
                    edgeOne, weights1, i1, false,
                    edgeTwo, weights2, i2, false,
                    hasRoad
                );
            }

            TriangulateEdgeStrip_Hills(
                edgeTwo, weights2, i1, false,
                end,     Weights2, i2, endCell.Shape == TerrainShape.Hills,
                hasRoad
            );
        }

        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float index, bool perturbY = false) {
            Terrain.AddTriangle(center, edge.V1, edge.V2, perturbY);
            Terrain.AddTriangle(center, edge.V2, edge.V3, perturbY);
            Terrain.AddTriangle(center, edge.V3, edge.V4, perturbY);
            Terrain.AddTriangle(center, edge.V4, edge.V5, perturbY);

            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            Terrain.AddTriangleCellData(indices, Weights1);
            Terrain.AddTriangleCellData(indices, Weights1);
            Terrain.AddTriangleCellData(indices, Weights1);
            Terrain.AddTriangleCellData(indices, Weights1);
        }

        private void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1,
            EdgeVertices e2, Color w2, float index2,
            bool hasRoad = false
        ) {
            Terrain.AddQuad(e1.V1, e1.V2, e2.V1, e2.V2);
            Terrain.AddQuad(e1.V2, e1.V3, e2.V2, e2.V3);
            Terrain.AddQuad(e1.V3, e1.V4, e2.V3, e2.V4);
            Terrain.AddQuad(e1.V4, e1.V5, e2.V4, e2.V5);

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);

            if(hasRoad) {
                TriangulateRoadSegment(e1.V2, e1.V3, e1.V4, e2.V2, e2.V3, e2.V4, w1, w2, indices);
            }
        }

        private void TriangulateEdgeStrip_Hills(
            EdgeVertices e1, Color w1, float index1, bool perturbEdgeOne,
            EdgeVertices e2, Color w2, float index2, bool perturbEdgeTwo,
            bool hasRoad = false
        ) {
            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V1, perturbEdgeOne), NoiseGenerator.Perturb(e1.V2, perturbEdgeOne),
                NoiseGenerator.Perturb(e2.V1, perturbEdgeTwo), NoiseGenerator.Perturb(e2.V2, perturbEdgeTwo)
            );

            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V2, perturbEdgeOne), NoiseGenerator.Perturb(e1.V3, perturbEdgeOne),
                NoiseGenerator.Perturb(e2.V2, perturbEdgeTwo), NoiseGenerator.Perturb(e2.V3, perturbEdgeTwo)
            );

            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V3, perturbEdgeOne), NoiseGenerator.Perturb(e1.V4, perturbEdgeOne),
                NoiseGenerator.Perturb(e2.V3, perturbEdgeTwo), NoiseGenerator.Perturb(e2.V4, perturbEdgeTwo)
            );

            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V4, perturbEdgeOne), NoiseGenerator.Perturb(e1.V5, perturbEdgeOne),
                NoiseGenerator.Perturb(e2.V4, perturbEdgeTwo), NoiseGenerator.Perturb(e2.V5, perturbEdgeTwo)
            );

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);

            if(hasRoad) {
                TriangulateRoadSegment(e1.V2, e1.V3, e1.V4, e2.V2, e2.V3, e2.V4, w1, w2, indices);
            }
        }

        private void TriangulateCorner(
            Vector3 bottom, IHexCell bottomCell, bool perturbBottomY,
            Vector3 left,   IHexCell leftCell,   bool perturbLeftY,
            Vector3 right,  IHexCell rightCell,  bool perturbRightY
        ) {
            HexEdgeType leftEdgeType  = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if(leftEdgeType == HexEdgeType.Slope) {

                if(rightEdgeType == HexEdgeType.Slope) {
                    TriangulateCornerTerraces(
                        bottom, bottomCell, perturbBottomY,
                        left,   leftCell,   perturbLeftY,
                        right,  rightCell,  perturbRightY
                    );

                }else if(rightEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(
                        left, leftCell,     perturbLeftY,
                        right, rightCell,   perturbRightY,
                        bottom, bottomCell, perturbBottomY
                    );
                }else {
                    TriangulateCornerTerracesCliff(
                        bottom, bottomCell, perturbBottomY,
                        left,   leftCell,   perturbLeftY,
                        right,  rightCell,  perturbRightY
                    );
                }

            }else if(rightEdgeType == HexEdgeType.Slope) {

                if(leftEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(
                        right,  rightCell,  perturbRightY,
                        bottom, bottomCell, perturbBottomY,
                        left,   leftCell,   perturbLeftY
                    );
                }else {
                    TriangulateCornerCliffTerraces(
                        bottom, bottomCell, perturbBottomY,
                        left,   leftCell,   perturbLeftY,
                        right,  rightCell,  perturbRightY
                    );
                }

            }else if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {

                if(leftCell.FoundationElevation < rightCell.FoundationElevation) {
                    TriangulateCornerCliffTerraces(
                        right,  rightCell,  perturbRightY,
                        bottom, bottomCell, perturbBottomY,
                        left,   leftCell,   perturbLeftY
                    );
                }else {
                    TriangulateCornerTerracesCliff(
                        left,   leftCell,   perturbLeftY,
                        right,  rightCell,  perturbRightY,
                        bottom, bottomCell, perturbBottomY
                    );
                }

            }else {
                Terrain.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(bottom, perturbBottomY),
                    NoiseGenerator.Perturb(left,   perturbLeftY  ),
                    NoiseGenerator.Perturb(right,  perturbRightY )
                );

                Vector3 indices;
                indices.x = bottomCell.Index;
                indices.y = leftCell  .Index;
                indices.z = rightCell .Index;

                Terrain.AddTriangleCellData(indices, Weights1, Weights2, Weights3);
            }            
        }

        private void TriangulateCornerTerraces(
            Vector3 begin, IHexCell beginCell, bool perturbBeginY,
            Vector3 left,  IHexCell leftCell,  bool perturbLeftY,
            Vector3 right, IHexCell rightCell, bool perturbRightY
        ){
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);

            Color w3 = HexMetrics.TerraceLerp(Weights1, Weights2, 1);
            Color w4 = HexMetrics.TerraceLerp(Weights1, Weights3, 1);

            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            Terrain.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(begin, perturbBeginY),
                NoiseGenerator.Perturb(v3),
                NoiseGenerator.Perturb(v4)
            );

            Terrain.AddTriangleCellData(indices, Weights1, w3, w4);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color w1 = w3;
                Color w2 = w4;

                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                w3 = HexMetrics.TerraceLerp(Weights1, Weights2, i);
                w4 = HexMetrics.TerraceLerp(Weights1, Weights3, i);

                Terrain.AddQuad(v1, v2, v3, v4);

                Terrain.AddQuadCellData(indices, w1, w2, w3, w4);
            }

            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(v3,    false),
                NoiseGenerator.Perturb(v4,    false),
                NoiseGenerator.Perturb(left,  perturbLeftY ),
                NoiseGenerator.Perturb(right, perturbRightY)
            );

            Terrain.AddQuadCellData(indices, w3, w4, Weights2, Weights3);
        }

        private void TriangulateCornerTerracesCliff(
            Vector3 begin, IHexCell beginCell, bool perturbBeginY,
            Vector3 left,  IHexCell leftCell,  bool perturbLeftY,
            Vector3 right, IHexCell rightCell, bool perturbRightY
        ){
            float b = Mathf.Abs(1f / (rightCell.FoundationElevation - beginCell.FoundationElevation));

            Vector3 boundary = Vector3.Lerp(
                NoiseGenerator.Perturb(begin, perturbBeginY),
                NoiseGenerator.Perturb(right, perturbRightY),
                b
            );

            Color boundaryWeights = Color.Lerp(Weights1, Weights3, b);

            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            TriangulateBoundaryTriangle(
                begin,    Weights1, perturbBeginY,
                left,     Weights2, perturbLeftY,
                boundary, boundaryWeights,
                indices
            );

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(
                    left,     Weights2, perturbLeftY,
                    right,    Weights3, perturbRightY,
                    boundary, boundaryWeights,
                    indices
                );
            }else {
                Terrain.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(left, perturbLeftY),
                    NoiseGenerator.Perturb(right, perturbRightY),
                    boundary
                );
                Terrain.AddTriangleCellData(indices, Weights2, Weights3, boundaryWeights);
            }
        }

        private void TriangulateCornerCliffTerraces(
            Vector3 begin, IHexCell beginCell, bool perturbBeginY,
            Vector3 left,  IHexCell leftCell,  bool perturbLeftY,
            Vector3 right, IHexCell rightCell, bool perturbRightY
        ){
            float b = Mathf.Abs(1f / (leftCell.FoundationElevation - beginCell.FoundationElevation));

            Vector3 boundary = Vector3.Lerp(
                NoiseGenerator.Perturb(begin, perturbBeginY),
                NoiseGenerator.Perturb(left, perturbLeftY),
                b
            );

            Color boundaryWeights = Color.Lerp(Weights1, Weights2, b);

            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            TriangulateBoundaryTriangle(
                right, Weights3, perturbRightY,
                begin, Weights1, perturbBeginY,
                boundary, boundaryWeights, indices
            );

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(
                    left,  Weights2, perturbLeftY,
                    right, Weights3, perturbRightY,
                    boundary, boundaryWeights, indices
                );
            }else {
                Terrain.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(left,  perturbLeftY),
                    NoiseGenerator.Perturb(right, perturbRightY),
                    boundary
                );

                Terrain.AddTriangleCellData(indices, Weights2, Weights3, boundaryWeights);
            }
        }

        private void TriangulateBoundaryTriangle(
            Vector3 begin, Color beginWeights, bool perturbBeginY,
            Vector3 left,  Color leftWeights,  bool perturbLeftY,
            Vector3 boundary, Color boundaryWeights,
            Vector3 indices
        ) {
            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);

            Terrain.AddTriangleUnperturbed(NoiseGenerator.Perturb(begin, perturbBeginY), v2, boundary);
            Terrain.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color w1 = w2;
                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);
                Terrain.AddTriangleUnperturbed(v1, v2, boundary);
                Terrain.AddTriangleCellData(indices, w1, w2, boundaryWeights);
            }

            Terrain.AddTriangleUnperturbed(v2, NoiseGenerator.Perturb(left, perturbLeftY), boundary);
            Terrain.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
        }

        private void TriangulateRiverQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y, float v, bool isReversed, Vector3 indices
        ) {
            TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, isReversed, indices);
        }

        private void TriangulateRiverQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2, float v, bool isReversed,
            Vector3 indices
        ) {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            Rivers.AddQuad(v1, v2, v3, v4);

            if(isReversed) {
                Rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
            }else {
                Rivers.AddQuadUV(0f, 1f,v, v + 0.2f);
            }

            Rivers.AddQuadCellData(indices, Weights1, Weights2);
        }

        private void TriangulateRoadSegment(
            Vector3 v1, Vector3 v2, Vector3 v3,
            Vector3 v4, Vector3 v5, Vector3 v6,
            Color w1, Color w2, Vector3 indices
        ) {
            Roads.AddQuad(v1, v2, v4, v5);
            Roads.AddQuad(v2, v3, v5, v6);

            Roads.AddQuadUV(0f, 1f, 0f, 0f);
            Roads.AddQuadUV(1f, 0f, 0f, 0f);

            Roads.AddQuadCellData(indices, w1, w2);
            Roads.AddQuadCellData(indices, w1, w2);
        }

        private void TriangulateRoad(
            Vector3 center, Vector3 middleLeft, Vector3 middleRight, EdgeVertices e,
            bool hasRoadThroughCellEdge, float index
        ) {
            if(hasRoadThroughCellEdge) {
                Vector3 middleCenter = Vector3.Lerp(middleLeft, middleRight, 0.5f);

                Vector3 indices;
                indices.x = indices.y = indices.z = index;

                TriangulateRoadSegment(
                    middleLeft, middleCenter, middleRight,
                    e.V2, e.V3, e.V4,
                    Weights1, Weights2, indices
                );

                Roads.AddTriangle(center, middleLeft, middleCenter);
                Roads.AddTriangle(center, middleCenter, middleRight);
                Roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f)
                );
                Roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f)
                );

                Roads.AddTriangleCellData(indices, Weights1);
                Roads.AddTriangleCellData(indices, Weights1);
            }else {
                TriangulateRoadEdge(center, middleLeft, middleRight, index);
            }            
        }

        private void TriangulateRoadEdge(Vector3 center, Vector3 middleLeft, Vector3 middleRight, float index) {
            Roads.AddTriangle(center, middleLeft, middleRight);
            Roads.AddTriangleUV(
                new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );

            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            Roads.AddTriangleCellData(indices, Weights1);
        }

        private void TriangulateRoadsAdjacentToRiver(
            HexDirection direction, IHexCell cell, Vector3 center, EdgeVertices e
        ){
            var previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());
            var neighbor         = Grid.GetNeighbor(cell, direction);
            var nextNeighbor     = Grid.GetNeighbor(cell, direction.Next());

            bool hasRoadThroughEdge = neighbor != null && neighbor.HasRoads;
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
                    if( !hasRoadThroughEdge && !previousNeighbor.HasRoads) {
                        return;
                    }
                    corner = HexMetrics.GetSecondOuterSolidCorner(direction);
                }else {
                    if( !hasRoadThroughEdge && !nextNeighbor.HasRoads) {
                        return;
                    }
                    corner = HexMetrics.GetFirstOuterSolidCorner(direction);
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

                var cellPrevToMiddle   = Grid.GetNeighbor(cell, middle.Previous());
                var cellAtMiddle       = Grid.GetNeighbor(cell, middle);
                var cellNextFromMiddle = Grid.GetNeighbor(cell, middle.Next());

                if( !(cellPrevToMiddle   == null || cellPrevToMiddle  .HasRoads) &&
                    !(cellAtMiddle       == null || cellAtMiddle      .HasRoads) &&
                    !(cellNextFromMiddle == null || cellNextFromMiddle.HasRoads)
                ) {
                    return;
                }

                roadCenter += HexMetrics.GetSolidEdgeMiddle(middle) * 0.25f;
            }

            Vector3 middleLeft  = Vector3.Lerp(roadCenter, e.V1, interpolators.x);
            Vector3 middleRight = Vector3.Lerp(roadCenter, e.V5, interpolators.y);
            TriangulateRoad(roadCenter, middleLeft, middleRight, e, hasRoadThroughEdge, cell.Index);

            if(previousHasRiver) {
                TriangulateRoadEdge(roadCenter, center, middleLeft, cell.Index);
            }
            if(nextHasRiver) {
                TriangulateRoadEdge(roadCenter, middleRight, center, cell.Index);
            }
        }

        private Vector2 GetRoadInterpolators(HexDirection direction, IHexCell cell) {
            var previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());
            var neighbor         = Grid.GetNeighbor(cell, direction);
            var nextNeighbor     = Grid.GetNeighbor(cell, direction.Next());

            Vector2 interpolators;
            if(ShouldRoadBeRendered(cell, neighbor)) {
                interpolators.x = interpolators.y = 0.5f;

            }else {
                interpolators.x = ShouldRoadBeRendered(cell, previousNeighbor) ? 0.5f : 0.25f;
                interpolators.y = ShouldRoadBeRendered(cell, nextNeighbor)     ? 0.5f : 0.25f;
            }
            return interpolators;
        }

        private bool ShouldRoadBeRendered(IHexCell cell, IHexCell neighbor) {
            return neighbor != null
                && neighbor.HasRoads
                && cell.HasRoads
                && HexMetrics.GetEdgeType(cell, neighbor) != HexEdgeType.Cliff;
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

            Vector3 indices;
            indices.x = indices.z = cell.Index;
            indices.y = neighbor.Index;
            Water.AddTriangleCellData(indices, Weights1);
            Water.AddTriangleCellData(indices, Weights1);
            Water.AddTriangleCellData(indices, Weights1);
            Water.AddTriangleCellData(indices, Weights1);

            Vector3 centerTwo = neighbor.transform.localPosition;
            centerTwo.y = center.y;

            EdgeVertices edgeTwo = new EdgeVertices(
                centerTwo + HexMetrics.GetSecondOuterSolidCorner(direction.Opposite()),
                centerTwo + HexMetrics.GetFirstOuterSolidCorner (direction.Opposite())
            );

            if(RiverCanon.HasRiverThroughEdge(cell, direction)) {
                TriangulateEstuary(edgeOne, edgeTwo, RiverCanon.GetIncomingRiver(cell) == direction, indices);
            }else {
                WaterShore.AddQuad(edgeOne.V1, edgeOne.V2, edgeTwo.V1, edgeTwo.V2);
                WaterShore.AddQuad(edgeOne.V2, edgeOne.V3, edgeTwo.V2, edgeTwo.V3);
                WaterShore.AddQuad(edgeOne.V3, edgeOne.V4, edgeTwo.V3, edgeTwo.V4);
                WaterShore.AddQuad(edgeOne.V4, edgeOne.V5, edgeTwo.V4, edgeTwo.V5);

                WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
                WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
                WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
                WaterShore.AddQuadUV(0f, 0f, 0f, 1f);

                WaterShore.AddQuadCellData(indices, Weights1, Weights2);
                WaterShore.AddQuadCellData(indices, Weights1, Weights2);
                WaterShore.AddQuadCellData(indices, Weights1, Weights2);
                WaterShore.AddQuadCellData(indices, Weights1, Weights2);
            }

            IHexCell nextNeighbor = Grid.GetNeighbor(cell, direction.Next());
            if(nextNeighbor != null) {
                Vector3 v3 = nextNeighbor.transform.localPosition + (
                    nextNeighbor.IsUnderwater ? 
                    HexMetrics.GetFirstWaterCorner(direction.Previous()) :
                    HexMetrics.GetFirstOuterSolidCorner(direction.Previous())
                );
                v3.y = center.y;

                WaterShore.AddTriangle(edgeOne.V5, edgeTwo.V5, v3);

                WaterShore.AddTriangleUV(
                    new Vector2(0f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f)
                );

                indices.z = nextNeighbor.Index;
                WaterShore.AddTriangleCellData(indices, Weights1, Weights2, Weights3);
            }
        }

        private void TriangulateOpenWater(
            HexDirection direction, IHexCell cell, IHexCell neighbor, Vector3 center
        ) {
            Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(direction);
            Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(direction);

            Water.AddTriangle(center, c1, c2);

            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            Water.AddTriangleCellData(indices, Weights1);

            if(direction <= HexDirection.SE && neighbor != null) {
                Vector3 bridge = HexMetrics.GetWaterBridge(direction);
                Vector3 e1 = c1 + bridge;
                Vector3 e2 = c2 + bridge;

                Water.AddQuad(c1, c2, e1, e2);

                indices.y = neighbor.Index;
                Water.AddQuadCellData(indices, Weights1, Weights2);

                if(direction <= HexDirection.E && Grid.HasNeighbor(cell, direction.Next())) {
                    var nextNeighbor = Grid.GetNeighbor(cell, direction.Next());
                    if(!nextNeighbor.IsUnderwater) {
                        return;
                    }

                    Water.AddTriangle(c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next()));

                    indices.z = nextNeighbor.Index;
                    Water.AddTriangleCellData(indices, Weights1, Weights2, Weights3);
                }
            }
        }

        private void TriangulateWaterfallInWater(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2, float waterY, Vector3 indices
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

            Rivers.AddQuadCellData(indices, Weights1, Weights2);
        }

        private void TriangulateEstuary(
            EdgeVertices edgeOne, EdgeVertices edgeTwo,
            bool incomingRiver, Vector3 indices
        ) {
            WaterShore.AddTriangle(edgeTwo.V1, edgeOne.V2, edgeOne.V1);
            WaterShore.AddTriangle(edgeTwo.V5, edgeOne.V5, edgeOne.V4);

            WaterShore.AddTriangleUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );
            WaterShore.AddTriangleUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );

            WaterShore.AddTriangleCellData(indices, Weights2, Weights1, Weights1);
            WaterShore.AddTriangleCellData(indices, Weights2, Weights1, Weights1);

            Estuaries.AddQuad    (edgeTwo.V1, edgeOne.V2, edgeTwo.V2, edgeOne.V3);
            Estuaries.AddTriangle(edgeOne.V3, edgeTwo.V2, edgeTwo.V4);
            Estuaries.AddQuad    (edgeOne.V3, edgeOne.V4, edgeTwo.V4, edgeTwo.V5);

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

            Estuaries.AddQuadCellData    (indices, Weights2, Weights1, Weights2, Weights1);
            Estuaries.AddTriangleCellData(indices, Weights1, Weights2, Weights2);
            Estuaries.AddQuadCellData    (indices, Weights1, Weights2);

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
                center + HexMetrics.GetFirstOuterSolidCorner(direction),
                center + HexMetrics.GetSecondOuterSolidCorner(direction)
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

            var edgeType = HexMetrics.GetEdgeType(cell, neighbor);
            if(edgeType == HexEdgeType.Slope) {
                TriangulateCultureTerraces(
                    edgeOne, cell, edgeTwo, neighbor, owner
                );
            }else {
                TriangulateCultureEdgeStrip(
                    edgeOne, Weights1, cell.Index, 
                    edgeTwo, Weights2, neighbor.Index,
                    owner, 0f, 1f
                );

                /*
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

                Culture.AddQuadCellData();
                */
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

            if(cell.FoundationElevation <= neighbor.FoundationElevation) {
                if(cell.FoundationElevation <= nextNeighbor.FoundationElevation) {
                    TriangulateCultureCorner(edgeOne.V5, cell, edgeTwo.V5, neighbor, v5, nextNeighbor, owner);
                }else {
                    TriangulateCultureCorner(v5, nextNeighbor, edgeOne.V5, cell, edgeTwo.V5, neighbor, owner);
                }
            }else if(neighbor.FoundationElevation <= nextNeighbor.FoundationElevation) {
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
            Color w2 = HexMetrics.TerraceLerp(Weights1, Weights2, 1);
            float i1 = beginCell.Index;
            float i2 = endCell.Index;

            TriangulateCultureEdgeStrip(begin, Weights1, i1, e2, w2, i2, owner, 0f, uv2);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices e1 = e2;
                float uv1 = uv2;
                Color w1 = w2;

                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                uv2 = HexMetrics.TerraceLerp(0f, 1f, i);
                w2 = HexMetrics.TerraceLerp(Weights1, Weights2, i);

                TriangulateCultureEdgeStrip(e1, w1, i1, e2, w2, i2, owner, uv1, uv2);
            }

            TriangulateCultureEdgeStrip(e2, w2, i1, end, Weights2, i2, owner, uv2, 1f);
        }

        private void TriangulateCultureEdgeStrip(
            EdgeVertices edgeOne, Color w1, float index1,
            EdgeVertices edgeTwo, Color w2, float index2,
            ICivilization owner, float vMin, float vMax
        ) {
            Culture.AddQuad(edgeOne.V1, edgeOne.V2, edgeTwo.V1, edgeTwo.V2);
            Culture.AddQuad(edgeOne.V2, edgeOne.V3, edgeTwo.V2, edgeTwo.V3);
            Culture.AddQuad(edgeOne.V3, edgeOne.V4, edgeTwo.V3, edgeTwo.V4);
            Culture.AddQuad(edgeOne.V4, edgeOne.V5, edgeTwo.V4, edgeTwo.V5);

            Culture.AddQuadColor(owner.Color);
            Culture.AddQuadColor(owner.Color);
            Culture.AddQuadColor(owner.Color);
            Culture.AddQuadColor(owner.Color);

            Culture.AddQuadUV(0f, 0f, vMin, vMax);
            Culture.AddQuadUV(0f, 0f, vMin, vMax);
            Culture.AddQuadUV(0f, 0f, vMin, vMax);
            Culture.AddQuadUV(0f, 0f, vMin, vMax);

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;

            Culture.AddQuadCellData(indices, w1, w2);
            Culture.AddQuadCellData(indices, w1, w2);
            Culture.AddQuadCellData(indices, w1, w2);
            Culture.AddQuadCellData(indices, w1, w2);
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

                if(leftCell.FoundationElevation < rightCell.FoundationElevation) {
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

            Color w3 = HexMetrics.TerraceLerp(Weights1, Weights2, 1);
            Color w4 = HexMetrics.TerraceLerp(Weights1, Weights3, 1);

            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            Culture.AddTriangle(begin, v3, v4);
            Culture.AddTriangleColor(owner.Color);
            Culture.AddTriangleUV(
                new Vector2(0f, uvMin),
                new Vector2(0f, uv3),
                new Vector2(0f, uv4)
            );

            Culture.AddTriangleCellData(indices, Weights1, w3, w4);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                float uv1 = uv3;
                float uv2 = uv4;
                Color w1 = w3;
                Color w2 = w4;

                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                uv3 = HexMetrics.TerraceLerp(uvMin, leftUVMax, i);
                uv4 = HexMetrics.TerraceLerp(uvMin, rightUVMax, i);
                w3 = HexMetrics.TerraceLerp(Weights1, Weights2, i);
                w4 = HexMetrics.TerraceLerp(Weights1, Weights3, i);

                Culture.AddQuad(v1, v2, v3, v4);
                Culture.AddQuadColor(owner.Color);
                Culture.AddQuadUV(
                    new Vector2(0f, uv1), new Vector2(0f, uv2),
                    new Vector2(0f, uv3), new Vector2(0f, uv4)
                );

                Culture.AddQuadCellData(indices, w1, w2, w3, w4);
            }

            Culture.AddQuad(v3, v4, left, right);
            Culture.AddQuadColor(owner.Color);
            Culture.AddQuadUV(
                new Vector2(0f, uv3), new Vector2(0f, uv4),
                new Vector2(0f, leftUVMax), new Vector2(0f, rightUVMax)
            );
            Culture.AddQuadCellData(indices, w3, w4, Weights2, Weights3);
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

            float b = Mathf.Abs(1f / (rightCell.FoundationElevation - beginCell.FoundationElevation));
            float boundaryUV = Mathf.Lerp(uvMin, rightUVMax, b);

            Vector3 boundary = Vector3.Lerp(NoiseGenerator.Perturb(begin), NoiseGenerator.Perturb(right), b);

            Color boundaryWeights = Color.Lerp(Weights1, Weights3, b);
            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell .Index;
            indices.z = rightCell.Index;

            TriangulateCultureBoundaryTriangle(
                begin, beginCell, uvMin,     Weights1,
                left, leftCell,   leftUVMax, Weights2,
                boundary, boundaryUV,
                boundaryWeights, indices,
                owner
            );

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateCultureBoundaryTriangle(
                    left,  leftCell,  leftUVMax,  Weights2,
                    right, rightCell, rightUVMax, Weights3,
                    boundary, boundaryUV,
                    boundaryWeights, indices,
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

                Culture.AddTriangleCellData(indices, Weights2, Weights3, boundaryWeights);
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

            float b = Mathf.Abs(1f / (leftCell.FoundationElevation - beginCell.FoundationElevation));
            float boundaryUV = Mathf.Lerp(uvMin, leftUVMax, b);

            Vector3 boundary = Vector3.Lerp(NoiseGenerator.Perturb(begin), NoiseGenerator.Perturb(left), b);

            Color boundaryWeights = Color.Lerp(Weights1, Weights2, b);
            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            TriangulateCultureBoundaryTriangle(
                right, rightCell, rightUVMax, Weights3,
                begin, beginCell, uvMin,      Weights1,
                boundary, boundaryUV,
                boundaryWeights, indices,
                owner
            );

            if(leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateCultureBoundaryTriangle(
                    left,  leftCell,  leftUVMax,  Weights2,
                    right, rightCell, rightUVMax, Weights3,
                    boundary, boundaryUV,
                    boundaryWeights, indices,
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
                Culture.AddTriangleCellData(indices, Weights2, Weights3, boundaryWeights);
            }
        }

        private void TriangulateCultureBoundaryTriangle(
            Vector3 begin, IHexCell beginCell, float beginUV, Color beginWeights,
            Vector3 left,  IHexCell leftCell,  float leftUV,  Color leftWeights,
            Vector3 boundary, float boundaryUV, Color boundaryWeights,
            Vector3 indices,
            ICivilization owner
        ) {
            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            float uv2 = HexMetrics.TerraceLerp(beginUV, leftUV, 1);
            Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);

            Culture.AddTriangleUnperturbed(NoiseGenerator.Perturb(begin), v2, boundary);
            Culture.AddTriangleColor(owner.Color);
            Culture.AddTriangleUV(
                new Vector2(0f, beginUV),
                new Vector2(0f, uv2),
                new Vector2(0f, boundaryUV)
            );
            Culture.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                float uv1 = uv2;
                Color w1 = w2;

                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                uv2 = HexMetrics.TerraceLerp(beginUV, leftUV, i);
                w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);

                Culture.AddTriangleUnperturbed(v1, v2, boundary);
                Culture.AddTriangleColor(owner.Color);
                Culture.AddTriangleUV(
                    new Vector2(0f, uv1),
                    new Vector2(0f, uv2),
                    new Vector2(0f, boundaryUV)
                );
                Culture.AddTriangleCellData(indices, w1, w2, boundaryWeights);
            }

            Culture.AddTriangleUnperturbed(v2, NoiseGenerator.Perturb(left), boundary);
            Culture.AddTriangleColor(owner.Color);
            Culture.AddTriangleUV(
                new Vector2(0f, uv2),
                new Vector2(0f, leftUV),
                new Vector2(0f, boundaryUV)
            );
            Culture.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
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

            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;
            Culture.AddTriangleCellData(indices, Weights1, Weights2, Weights3);
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
