﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.WorkerSlots;

using UnityCustomUtilities.DataStructures;

namespace Assets.Simulation.HexMap {

    public class HexGrid : MonoBehaviour, IHexGrid {

        #region static fields and properties

        private static Vector3 MapIntersector = new Vector3(0f, 20f, 0f);

        #endregion

        #region instance fields and properties

        #region from IHexGrid

        public ReadOnlyCollection<IHexCell> Cells {
            get { return cells.AsReadOnly(); }
        }
        private List<IHexCell> cells;

        public int ChunkCountX { get; private set; }
        public int ChunkCountZ { get; private set; }

        public int CellCountX { get; private set; }
        public int CellCountZ { get; private set; }

        #endregion

        [SerializeField] private HexCell CellPrefab;

        [SerializeField] private HexGridChunk ChunkPrefab;

        [SerializeField] private LayerMask TerrainCollisionMask;        

        private HexGridChunk[] Chunks;

        private HexCellShaderData CellShaderData;



        private DiContainer            Container;
        private IWorkerSlotFactory     WorkerSlotFactory;
        private ICellModificationLogic CellModificationLogic;
        private IHexMapRenderConfig    RenderConfig;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            DiContainer container, IWorkerSlotFactory workerSlotFactory,
            ICellModificationLogic cellModificationLogic,
            IHexMapRenderConfig renderConfig
        ) {
            Container             = container;
            WorkerSlotFactory     = workerSlotFactory;
            CellModificationLogic = cellModificationLogic;
            RenderConfig          = renderConfig;
        }

        #region Unity message methods

        #endregion

        #region from IHexGrid        

        public void Build(int chunkCountX, int chunkCountZ) {
            Clear();

            ChunkCountX = chunkCountX;
            ChunkCountZ = chunkCountZ;

            CellCountX = ChunkCountX * RenderConfig.ChunkSizeX;
            CellCountZ = ChunkCountZ * RenderConfig.ChunkSizeZ;

            CellShaderData = Container.InstantiateComponent<HexCellShaderData>(gameObject);
            CellShaderData.Initialize(CellCountX, CellCountZ);

            CellShaderData.enabled = true;

            var oldVisibilityMode = CellShaderData.ImmediateMode;

            CellShaderData.ImmediateMode = true;

            CreateChunks();
            CreateCells();
            ToggleUI(false);

            StartCoroutine(RevertVisibilityMode(oldVisibilityMode));
        }

        public void Clear() {
            if(cells == null) {
                return;
            }

            for(int i = Cells.Count - 1; i >= 0; i--) {
                Cells[i].Destroy();
            }

            for(int i = Chunks.Length - 1; i >= 0; i--) {
                Destroy(Chunks[i].gameObject);
            }

            cells  = null;
            Chunks = null;
        }

        public bool HasCellAtCoordinates(HexCoordinates coordinates) {
            int expectedIndex = coordinates.X + coordinates.Z * CellCountX + coordinates.Z / 2;

            return expectedIndex >= 0
                && expectedIndex < Cells.Count
                && Cells[expectedIndex].Coordinates.Equals(coordinates);
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
            if(HasNeighbor(center, direction)) {
                return GetCellAtCoordinates(HexCoordinates.GetNeighborInDirection(center.Coordinates, direction));
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
            if(center == null) {
                throw new ArgumentNullException("center");
            }

            var retval = new List<IHexCell>();

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
                start.Coordinates, start.Position,
                end.Coordinates, end.Position
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

        public List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end) {
            return GetShortestPathBetween(start, end, (a, b) => 1);
        }

        public List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end, Func<IHexCell, IHexCell, float> costFunction) {
            if(start == null) {
                throw new ArgumentNullException("start");
            }else if(end == null) {
                throw new ArgumentNullException("end");
            }

            HexCoordinates startCoords = start.Coordinates;
            HexCoordinates endCoords = end.Coordinates;
            PriorityQueue<HexCoordinates> frontier = new PriorityQueue<HexCoordinates>();
            frontier.Add(startCoords, 0);
            Dictionary<HexCoordinates, HexCoordinates> cameFrom = new Dictionary<HexCoordinates, HexCoordinates>();
            Dictionary<HexCoordinates, float> costSoFar = new Dictionary<HexCoordinates, float>();
            cameFrom[startCoords] = null;
            costSoFar[startCoords] = 0;

            IHexCell nextHex;
            HexCoordinates current = null;
            while(frontier.Count() > 0) {
                current = frontier.DeleteMin();
                if(current.Equals(endCoords)) break;

                foreach(var nextCoords in HexCoordinates.GetCoordinatesInRing(current, 1)) {
                    if(!HasCellAtCoordinates(nextCoords)) {
                        continue;
                    }
                    nextHex = GetCellAtCoordinates(nextCoords);

                    float cost = costFunction(GetCellAtCoordinates(current), nextHex);
                    if(cost < 0) {
                        continue;
                    }
                    float newCost = costSoFar[current] + cost;

                    if(!costSoFar.ContainsKey(nextCoords) || newCost < costSoFar[nextCoords]) {
                        costSoFar[nextCoords] = newCost;
                        frontier.Add(nextCoords, newCost);
                        cameFrom[nextCoords] = current;
                    }
                }
            }

            if(cameFrom.ContainsKey(endCoords)) {
                var results = new List<IHexCell>();
                var lastHex = GetCellAtCoordinates(endCoords);
                results.Add(lastHex);
                HexCoordinates pathAncestor = cameFrom[endCoords];
                while(pathAncestor != startCoords) {
                    var currentHex = GetCellAtCoordinates(pathAncestor);
                    results.Add(currentHex);
                    pathAncestor = cameFrom[pathAncestor];
                }
                results.Reverse();
                return results;
            } else {
                return null;
            }
        }

        public List<IHexCell> GetNeighbors(IHexCell center) {
            return GetCellsInRadius(center, 1);
        }

        public Vector3 PerformIntersectionWithTerrainSurface(Vector3 xzPosition) {
            RaycastHit results;

            if(!Physics.Raycast(xzPosition + MapIntersector, Vector3.down * 50, out results, Mathf.Infinity, layerMask: TerrainCollisionMask)) {
                Debug.LogError("PerformIntersectionWithTerrainSurface failed to find a collision for xzPosition " + xzPosition.ToString());
            }

            return results.point;
        }

        public bool TryPerformIntersectionWithTerrainSurface(Vector3 xzPosition, out Vector3 hitpoint) {
            RaycastHit results;

            if(Physics.Raycast(xzPosition + MapIntersector, Vector3.down * 50, out results, Mathf.Infinity, layerMask: TerrainCollisionMask)) {
                hitpoint = results.point;
                return true;
            }else {
                hitpoint = Vector3.zero;
                return false;
            }
        }

        public void ToggleUI(bool isVisible) {
            foreach(var cell in Cells) {
                cell.Overlay.SetDisplayType(UI.HexMap.CellOverlayType.Labels);
                if(isVisible) {
                    cell.Overlay.Show();
                }else {
                    cell.Overlay.Hide();
                }
            }
        }

        #endregion

        private void CreateChunks() {
            Chunks = new HexGridChunk[ChunkCountX * ChunkCountZ];

            for(int z = 0, i = 0; z < ChunkCountZ; z++) {
                for(int x = 0; x < ChunkCountX; x++) {
                    HexGridChunk chunk = Chunks[i++] = Container.InstantiatePrefabForComponent<HexGridChunk>(ChunkPrefab);
                    chunk.transform.SetParent(transform);
                }
            }
        }

        private void CreateCells() {
            var newCells = new HexCell[CellCountX * CellCountZ];

            for(int z = 0, i = 0; z < CellCountZ; ++ z) {
                for(int x = 0; x < CellCountX; ++x) {
                    CreateCell(x, z, i++, newCells);
                }
            }

            cells = newCells.Cast<IHexCell>().ToList();
        }

        private void CreateCell(int x, int z, int i, HexCell[] newCells) {
            var position = new Vector3(
                (x + z * 0.5f - z / 2) * RenderConfig.InnerRadius * 2f,
                0f,
                z * RenderConfig.OuterRadius * 1.5f
            );

            var newCell = newCells[i] = Container.InstantiatePrefabForComponent<HexCell>(CellPrefab);

            newCell.transform.localPosition = position;

            newCell.WorkerSlot = WorkerSlotFactory.BuildSlot(newCell);

            newCell.ShaderData  = CellShaderData;
            newCell.Index       = i;
            newCell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

            CellModificationLogic.ChangeTerrainOfCell   (newCell, CellTerrain.Grassland);
            CellModificationLogic.ChangeShapeOfCell     (newCell, CellShape.Flatlands);
            CellModificationLogic.ChangeVegetationOfCell(newCell, CellVegetation.None);            

            newCell.gameObject.name = string.Format("Cell {0}", newCell.Coordinates);

            AddCellToChunk(x, z, newCell);
        }

        private void AddCellToChunk(int x, int z, HexCell cell) {
            int chunkX = x / RenderConfig.ChunkSizeX;
            int chunkZ = z / RenderConfig.ChunkSizeZ;
            HexGridChunk chunk = Chunks[chunkX + chunkZ * ChunkCountX];

            int localX = x - chunkX * RenderConfig.ChunkSizeX;
            int localZ = z - chunkZ * RenderConfig.ChunkSizeZ;

            chunk.AddCell(localX + localZ * RenderConfig.ChunkSizeX, cell);

            cell.Chunk = chunk;
        }

        private IEnumerator RevertVisibilityMode(bool oldVisibilityMode) {
            yield return new WaitForEndOfFrame();
            CellShaderData.ImmediateMode = oldVisibilityMode;
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
