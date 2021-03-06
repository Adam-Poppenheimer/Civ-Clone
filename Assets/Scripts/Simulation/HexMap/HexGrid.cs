﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.WorkerSlots;
using Assets.Simulation.MapRendering;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class HexGrid : MonoBehaviour, IHexGrid {

        #region instance fields and properties

        #region from IHexGrid

        public ReadOnlyCollection<IHexCell> Cells {
            get {
                if(_cellsReadOnlyWrapper == null) {
                    _cellsReadOnlyWrapper = cells.AsReadOnly();
                }

                return _cellsReadOnlyWrapper;
            }
        }
        private ReadOnlyCollection<IHexCell> _cellsReadOnlyWrapper;
        private List<IHexCell> cells = new List<IHexCell>();

        public int CellCountX { get; private set; }
        public int CellCountZ { get; private set; }

        public IEnumerable<IMapChunk> Chunks {
            get {
                if(castChunks == null && chunks != null) {
                    castChunks = chunks.Cast<IMapChunk>();
                }

                return castChunks;
            }
        }
        private IEnumerable<IMapChunk> castChunks;
        private MapChunk[,] chunks;

        public IHexMesh RiverSurfaceMesh { get; private set; }
        public IHexMesh RiverBankMesh    { get; private set; }
        public IHexMesh RiverDuckMesh    { get; private set; }
        public IHexMesh FarmMesh         { get; private set; }

        #endregion

        private int ChunkCountX;
        private int ChunkCountZ;




        private IWorkerSlotFactory     WorkerSlotFactory;
        private ICellModificationLogic CellModificationLogic;
        private IMapRenderConfig       RenderConfig;
        private HexCellSignals         CellSignals;
        private IGeometry2D            Geometry2D;
        private IHexCellShaderData     ShaderData;
        private IMemoryPool<MapChunk>  MapChunkPool;
        private IHexMeshFactory        HexMeshFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IWorkerSlotFactory workerSlotFactory, ICellModificationLogic cellModificationLogic,
            IMapRenderConfig renderConfig, HexCellSignals cellSignals, IGeometry2D geometry2D,
            IHexCellShaderData shaderData, IMemoryPool<MapChunk> mapChunkPool,
            IHexMeshFactory hexMeshFactory
        ) {
            WorkerSlotFactory     = workerSlotFactory;
            CellModificationLogic = cellModificationLogic;
            RenderConfig          = renderConfig;
            CellSignals           = cellSignals;
            Geometry2D            = geometry2D;
            ShaderData            = shaderData;
            MapChunkPool          = mapChunkPool;
            HexMeshFactory        = hexMeshFactory;
        }

        #region Unity messages

        private void OnDestroy() {
            Clear();
        }

        #endregion

        #region from IHexGrid        

        public void Build(int cellCountX, int cellCountZ) {
            RiverSurfaceMesh = HexMeshFactory.Create("River Surfaces", RenderConfig.RiverSurfaceData);
            RiverBankMesh    = HexMeshFactory.Create("River Banks",    RenderConfig.RiverBankData);
            RiverDuckMesh    = HexMeshFactory.Create("River Ducking",  RenderConfig.RiverDuckData);
            FarmMesh         = HexMeshFactory.Create("Farmland",       RenderConfig.FarmlandData);

            RiverSurfaceMesh.transform.SetParent(transform, false);
            RiverBankMesh   .transform.SetParent(transform, false);
            RiverDuckMesh   .transform.SetParent(transform, false);
            FarmMesh        .transform.SetParent(transform, false);

            Clear();

            CellCountX = cellCountX;
            CellCountZ = cellCountZ;

            ShaderData.Initialize(CellCountX, CellCountZ);

            CreateCells();
            CreateChunks();
            AttachChunksToCells();            
        }

        public void Clear() {
            if(cells == null || cells.Count == 0) {
                return;
            }

            CellSignals.MapBeingClearedSignal.OnNext(new UniRx.Unit());

            cells.Clear();

            foreach(var chunk in chunks) {
                MapChunkPool.Despawn(chunk);
            }

            chunks     = null;
            castChunks = null;

            if(RiverSurfaceMesh != null) { HexMeshFactory.Destroy(RiverSurfaceMesh); }
            if(RiverBankMesh    != null) { HexMeshFactory.Destroy(RiverBankMesh);    }
            if(RiverDuckMesh    != null) { HexMeshFactory.Destroy(RiverDuckMesh);    }
            if(FarmMesh         != null) { HexMeshFactory.Destroy(FarmMesh);         }

            RiverSurfaceMesh = null;
            RiverBankMesh    = null;
            RiverDuckMesh    = null;
            FarmMesh         = null;
        }

        public bool HasCellAtCoordinates(HexCoordinates coordinates) {
            int expectedIndex = coordinates.X + coordinates.Z * CellCountX + coordinates.Z / 2;

            return expectedIndex >= 0
                && expectedIndex < cells.Count
                && cells[expectedIndex].Coordinates.Equals(coordinates);
        }

        public IHexCell GetCellAtCoordinates(HexCoordinates coordinates) {
            int index = coordinates.X + coordinates.Z * CellCountX + coordinates.Z / 2;

            if(index < 0 || index >= Cells.Count) {
                throw new IndexOutOfRangeException("The given coordinates represent a cell that's not in the grid");
            }

            return Cells[index];
        }

        public bool HasCellAtLocation(Vector3 location) {
		    HexCoordinates coordinates = GetCoordinatesFromPosition(transform.InverseTransformPoint(location));

            return HasCellAtCoordinates(coordinates);
        }

        public IHexCell GetCellAtLocation(Vector3 location) {
            HexCoordinates coordinates = GetCoordinatesFromPosition(transform.InverseTransformPoint(location));

            return GetCellAtCoordinates(coordinates);
        }

        public IHexCell GetCellAtOffset(int xOffset, int zOffset) {
            return Cells[xOffset + zOffset * CellCountX];
        }

        public bool HasNeighbor(IHexCell center, HexDirection direction) {
            return HasCellAtCoordinates(HexCoordinates.GetNeighborInDirection(center.Coordinates, direction));
        }

        public IHexCell GetNeighbor(IHexCell center, HexDirection direction) {
            var neighborCoord = HexCoordinates.GetNeighborInDirection(center.Coordinates, direction);

            int expectedIndex = neighborCoord.X + neighborCoord.Z * CellCountX + neighborCoord.Z / 2;

            if(expectedIndex >= 0 && expectedIndex < cells.Count && cells[expectedIndex].Coordinates.Equals(neighborCoord)) {
                return cells[expectedIndex];
            }else {
                return null;
            }      
        }

        public int GetDistance(IHexCell cellOne, IHexCell cellTwo) {
            if(cellOne == null) {
                throw new ArgumentNullException("cellOne");
            }else if(cellTwo == null) {
                throw new ArgumentNullException("cellTwo");
            }

            return HexCoordinates.GetDistanceBetween(cellOne.Coordinates, cellTwo.Coordinates);
        }

        public List<IHexCell> GetCellsInRadius(IHexCell center, int radius) {
            return GetCellsInRadius(center, radius, false);
        }

        public List<IHexCell> GetCellsInRadius(IHexCell center, int radius, bool includeCenter) {
            if(center == null) {
                throw new ArgumentNullException("center");
            }

            var retval = new List<IHexCell>();

            if(includeCenter) {
                retval.Add(center);
            }

            foreach(var coordinates in HexCoordinates.GetCoordinatesInRadius(center.Coordinates, radius)) {
                if(HasCellAtCoordinates(coordinates)) {
                    retval.Add(GetCellAtCoordinates(coordinates));
                }
            }

            return retval;
        }

        public List<IHexCell> GetCellsInLine(IHexCell start, IHexCell end) {
            var retval = new List<IHexCell>();

            var coordinateLine = GetCoordinatesInLine(
                start.Coordinates, start.AbsolutePosition,
                end.Coordinates, end.AbsolutePosition
            );

            foreach(var coordinates in coordinateLine){
                if(HasCellAtCoordinates(coordinates)) {
                    retval.Add(GetCellAtCoordinates(coordinates));
                }
            }
            
            return retval;
        }

        public List<IHexCell> GetCellsInRing(IHexCell center, int radius) {
            if(center == null) {
                throw new ArgumentNullException("center");
            }

            var coordsInRing = HexCoordinates.GetCoordinatesInRing(center.Coordinates, radius);
            return coordsInRing.Where (coord => HasCellAtCoordinates(coord))
                               .Select(coord => GetCellAtCoordinates(coord))
                               .ToList();
        }

        public List<IHexCell> GetNeighbors(IHexCell center) {
            return GetCellsInRing(center, 1);
        }

        public Vector3 PerformIntersectionWithTerrainSurface(Vector3 xzPosition) {
            Vector3 hitpoint;

            if(!TryPerformIntersectionWithTerrainSurface(xzPosition, out hitpoint)) {
                Debug.LogError("PerformIntersectionWithTerrainSurface failed to find a collision for xzPosition " + xzPosition.ToString());
            }

            return hitpoint;
        }

        public bool TryPerformIntersectionWithTerrainSurface(Vector3 xzPosition, out Vector3 hitpoint) {
            if(!HasCellAtLocation(xzPosition)) {
                hitpoint = Vector3.zero;
                return false;
            }

            var cellAtLocation = GetCellAtLocation(xzPosition);

            foreach(var chunk in cellAtLocation.OverlappingChunks) {
                if(chunk.IsInTerrainBounds2D(xzPosition.ToXZ())) {
                    hitpoint = new Vector3(
                        xzPosition.x, chunk.Terrain.SampleHeight(xzPosition), xzPosition.z
                    );

                    return true;
                }
            }

            hitpoint = Vector3.zero;
            return false;
        }

        public Vector3 GetAbsolutePositionFromRelative(Vector3 relativePosition) {
            return transform.TransformPoint(relativePosition);
        }

        public bool TryGetSextantOfPointInCell(Vector2 xzPoint, IHexCell cell, out HexDirection sextant) {
            sextant = HexDirection.NE;

            foreach(var candidate in EnumUtil.GetValues<HexDirection>()) {
                sextant = candidate;

                if(Geometry2D.IsPointWithinTriangle(
                    xzPoint, cell.AbsolutePositionXZ,
                    cell.AbsolutePositionXZ + RenderConfig.GetFirstCornerXZ (candidate),
                    cell.AbsolutePositionXZ + RenderConfig.GetSecondCornerXZ(candidate)
                )) {
                    return true;
                }
            }

            return false;
        }

        #endregion

        private void CreateCells() {
            for(int z = 0, i = 0; z < CellCountZ; ++ z) {
                for(int x = 0; x < CellCountX; ++x) {
                    CreateCell(x, z, i++);
                }
            }
        }

        private void CreateCell(int x, int z, int i) {
            var position = new Vector3(
                (x + z * 0.5f - z / 2) * RenderConfig.InnerRadius * 2f,
                0f,
                z * RenderConfig.OuterRadius * 1.5f
            );

            var newCell = new HexCell(position, this, CellSignals);

            newCell.WorkerSlot = WorkerSlotFactory.BuildSlot(newCell);

            newCell.Index       = i;
            newCell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

            CellModificationLogic.ChangeTerrainOfCell   (newCell, CellTerrain.Grassland);
            CellModificationLogic.ChangeShapeOfCell     (newCell, CellShape.Flatlands);
            CellModificationLogic.ChangeVegetationOfCell(newCell, CellVegetation.None);

            cells.Add(newCell);
        }

        private void CreateChunks() {
            float mapWidth  = CellCountX * RenderConfig.InnerRadius * 2    + RenderConfig.InnerRadius;
            float mapHeight = CellCountZ * RenderConfig.OuterRadius * 1.5f + RenderConfig.OuterRadius / 2f;

            if(RenderConfig.ChunkWidth == 0f || RenderConfig.ChunkHeight == 0f) {
                throw new InvalidOperationException("Attempting to create chunks when at least one chunk dimension is zero");
            }

            ChunkCountX = Mathf.CeilToInt(mapWidth  / RenderConfig.ChunkWidth);
            ChunkCountZ = Mathf.CeilToInt(mapHeight / RenderConfig.ChunkHeight);

            chunks = new MapChunk[ChunkCountX, ChunkCountZ];
            castChunks = null;

            int chunkIndexX = 0, chunkIndexZ = 0;

            for(float chunkX = 0f; chunkX < mapWidth; chunkX += RenderConfig.ChunkWidth, chunkIndexX++) {
                chunkIndexZ = 0;

                for(float chunkZ = 0f; chunkZ < mapHeight; chunkZ += RenderConfig.ChunkHeight, chunkIndexZ++) {
                    float chunkWidth  = Mathf.Min(RenderConfig.ChunkWidth,  mapWidth  - chunkX);
                    float chunkHeight = Mathf.Min(RenderConfig.ChunkHeight, mapHeight - chunkZ);

                    var newChunk = CreateChunk(chunkX - RenderConfig.InnerRadius, chunkZ - RenderConfig.OuterRadius, chunkWidth, chunkHeight);

                    chunks[chunkIndexX, chunkIndexZ] = newChunk;

                    newChunk.gameObject.name = string.Format("Chunk[{0}, {1}]", chunkIndexX, chunkIndexZ);
                }
            }

            for(chunkIndexZ = 0; chunkIndexZ < ChunkCountZ; chunkIndexZ++) {
                for(chunkIndexX = 0; chunkIndexX < ChunkCountX; chunkIndexX++) {

                    var centerChunk = chunks[chunkIndexX, chunkIndexZ];

                    Terrain northNeighbor = chunkIndexZ < ChunkCountZ - 1 ? chunks[chunkIndexX,     chunkIndexZ + 1].Terrain : null;
                    Terrain eastNeighbor  = chunkIndexX < ChunkCountX - 1 ? chunks[chunkIndexX + 1, chunkIndexZ    ].Terrain : null;
                    Terrain southNeighbor = chunkIndexZ > 0               ? chunks[chunkIndexX,     chunkIndexZ - 1].Terrain : null;
                    Terrain westNeighbor  = chunkIndexX > 0               ? chunks[chunkIndexX - 1, chunkIndexZ    ].Terrain : null;

                    centerChunk.Terrain.SetNeighbors(
                        westNeighbor, northNeighbor, eastNeighbor, southNeighbor
                    );
                }
            }
        }

        private MapChunk CreateChunk(float chunkX, float chunkZ, float width, float height) {
            var newChunk = MapChunkPool.Spawn();

            newChunk.transform.SetParent(transform, false);

            newChunk.Initialize(
                new Vector3(chunkX, 0f, chunkZ), width, height
            );

            return newChunk;
        }

        private void AttachChunksToCells() {
            foreach(var cell in Cells) {
                int chunkIndexX = Mathf.FloorToInt(cell.AbsolutePosition.x / RenderConfig.ChunkWidth);
                int chunkIndexZ = Mathf.FloorToInt(cell.AbsolutePosition.z / RenderConfig.ChunkHeight);

                var overlappingChunks = new List<IMapChunk>();

                for(int i = -1; i <= 1; i++) {
                    for(int j = -1; j <= 1; j++) {
                        int candidateX = Math.Max(0, Math.Min(chunkIndexX + i, ChunkCountX - 1));
                        int candidateZ = Math.Max(0, Math.Min(chunkIndexZ + j, ChunkCountZ - 1));

                        IMapChunk candidate = chunks[candidateX, candidateZ];

                        if(candidate.DoesCellOverlapChunk(cell)) {
                            overlappingChunks.Add(candidate);
                        }
                    }
                }

                cell.AttachToChunks(overlappingChunks);

                foreach(var chunk in overlappingChunks) {
                    chunk.AttachCell(cell, chunk.IsInTerrainBounds2D(cell.AbsolutePositionXZ));
                }
            }
        }

        private HexCoordinates GetCoordinatesFromPosition(Vector3 position) {
            float x = position.x / (RenderConfig.InnerRadius * 2f);
            float y = -x;

            float offset = position.z / (RenderConfig.OuterRadius * 3f);

            x -= offset;
            y -= offset;

            int roundedX = Mathf.RoundToInt(x);
            int roundedY = Mathf.RoundToInt(y);
            int roundedZ = Mathf.RoundToInt(-x - y);

            if(roundedX + roundedY + roundedZ != 0) {
                float deltaX = Mathf.Abs(x - roundedX);
                float deltaY = Mathf.Abs(y - roundedY);
                float deltaZ = Mathf.Abs(-x - y - roundedZ);

                if(deltaX > deltaY && deltaX > deltaZ) {
                    roundedX = -roundedY - roundedZ;
                }else if(deltaZ > deltaY) {
                    roundedZ = -roundedX - roundedY;
                }
            }

            return new HexCoordinates(roundedX, roundedZ);
        }

        private List<HexCoordinates> GetCoordinatesInLine(
            HexCoordinates start, Vector3 startPosition,
            HexCoordinates end, Vector3 endPosition
        ){
            int distanceBetween = HexCoordinates.GetDistanceBetween(start, end);

            var results = new List<HexCoordinates>();
            float step = 1.0f / Mathf.Max(distanceBetween, 1);

            for(int i = 0; i < distanceBetween; i++) {
                results.Add(GetCoordinatesFromPosition(
                    Vector3.Lerp(startPosition, endPosition, step * i)
                ));
            }

            return results;
        }

        #endregion

    }

}
