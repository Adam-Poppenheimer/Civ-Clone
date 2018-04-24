using System;
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

        public ReadOnlyCollection<IHexCell> AllCells {
            get { return Cells.AsReadOnly(); }
        }

        public int ChunkCountX {
            get { return _chunkCountX; }
        }
        [SerializeField] private int _chunkCountX;

        public int ChunkCountZ {
            get { return _chunkCountZ; }
        }
        [SerializeField] private int _chunkCountZ;

        #endregion

        [SerializeField] private HexCell CellPrefab;

        [SerializeField] private HexGridChunk ChunkPrefab;

        [SerializeField] private LayerMask TerrainCollisionMask;

        private int CellCountX;
        private int CellCountZ;

        private List<IHexCell> Cells;

        private HexGridChunk[] Chunks;

        private HexCellShaderData CellShaderData;



        private IWorkerSlotFactory WorkerSlotFactory;
        private DiContainer        Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(DiContainer container, IWorkerSlotFactory workerSlotFactory){
            Container         = container;
            WorkerSlotFactory = workerSlotFactory;
        }

        #region Unity message methods

        private void Awake() {
            CellCountX = ChunkCountX * HexMetrics.ChunkSizeX;
            CellCountZ = ChunkCountZ * HexMetrics.ChunkSizeZ;

            CellShaderData = Container.InstantiateComponent<HexCellShaderData>(gameObject);
            CellShaderData.Initialize(CellCountX, CellCountZ);

            CellShaderData.enabled = true;
        }

        #endregion

        #region from IHexGrid        

        public void Build() {
            var oldVisibilityMode = CellShaderData.ImmediateMode;

            CellShaderData.ImmediateMode = true;

            CreateChunks();
            CreateCells();
            ToggleUI(false);

            StartCoroutine(RevertVisibilityMode(oldVisibilityMode));
        }

        public void Clear() {
            if(Cells == null) {
                return;
            }

            for(int i = Cells.Count - 1; i >= 0; i--) {
                Destroy(Cells[i].transform.gameObject);
            }

            for(int i = Chunks.Length - 1; i >= 0; i--) {
                Destroy(Chunks[i].gameObject);
            }

            Cells  = null;
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
		    HexCoordinates coordinates = HexCoordinates.FromPosition(transform.InverseTransformPoint(location));

            return HasCellAtCoordinates(coordinates);
        }

        public IHexCell GetCellAtLocation(Vector3 location) {
            HexCoordinates coordinates = HexCoordinates.FromPosition(transform.InverseTransformPoint(location));

            return GetCellAtCoordinates(coordinates);
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

            var coordinateLine = HexCoordinates.GetCoordinatesInLine(
                start.Coordinates, start.transform.position,
                end.Coordinates, end.transform.position
            );

            foreach(var coordinates in coordinateLine){
                if(HasCellAtCoordinates(coordinates)) {
                    retval.Add(GetCellAtCoordinates(coordinates));
                }
            }
            
            return retval;
        }

        public List<IHexCell> GetCellsInRing(IHexCell center, int radius) {
            throw new NotImplementedException();
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

            Physics.Raycast(xzPosition + MapIntersector, Vector3.down, out results, TerrainCollisionMask);

            return results.point;
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

            Cells = newCells.Cast<IHexCell>().ToList();
        }

        private void CreateCell(int x, int z, int i, HexCell[] newCells) {
            var position = new Vector3(
                (x + z * 0.5f - z / 2) * HexMetrics.InnerRadius * 2f,
                0f,
                z * HexMetrics.OuterRadius * 1.5f
            );

            var newCell = newCells[i] = Container.InstantiatePrefabForComponent<HexCell>(CellPrefab);

            newCell.transform.localPosition = position;

            newCell.WorkerSlot = WorkerSlotFactory.BuildSlot(newCell);

            newCell.ShaderData  = CellShaderData;
            newCell.Index       = i;
            newCell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            newCell.Terrain     = TerrainType.Grassland;
            newCell.Feature     = TerrainFeature.None;
            newCell.FoundationElevation   = 0;

            

            newCell.gameObject.name = string.Format("Cell {0}", newCell.Coordinates);

            AddCellToChunk(x, z, newCell);
        }

        private void AddCellToChunk(int x, int z, HexCell cell) {
            int chunkX = x / HexMetrics.ChunkSizeX;
            int chunkZ = z / HexMetrics.ChunkSizeZ;
            HexGridChunk chunk = Chunks[chunkX + chunkZ * ChunkCountX];

            int localX = x - chunkX * HexMetrics.ChunkSizeX;
            int localZ = z - chunkZ * HexMetrics.ChunkSizeZ;

            chunk.AddCell(localX + localZ * HexMetrics.ChunkSizeX, cell);

            cell.Chunk = chunk;
        }

        private IEnumerator RevertVisibilityMode(bool oldVisibilityMode) {
            yield return new WaitForEndOfFrame();
            CellShaderData.ImmediateMode = oldVisibilityMode;
        }

        #endregion

    }

}
