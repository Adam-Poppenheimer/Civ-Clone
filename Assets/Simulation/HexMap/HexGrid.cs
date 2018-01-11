using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using UnityCustomUtilities.DataStructures;

namespace Assets.Simulation.HexMap {

    public class HexGrid : MonoBehaviour, IHexGrid {

        #region instance fields and properties

        #region from IHexGrid

        public ReadOnlyCollection<IHexCell> Tiles {
            get {
                if(_tiles == null) {
                    _tiles = GetComponentsInChildren<IHexCell>().ToList();
                }
                return _tiles.AsReadOnly();
            }
        }
        private List<IHexCell> _tiles;

        public int ChunkCountX {
            get { return _chunkCountX; }
        }
        [SerializeField] private int _chunkCountX;

        public int ChunkCountZ {
            get { return _chunkCountZ; }
        }
        [SerializeField] private int _chunkCountZ;

        #endregion        

        [SerializeField] private Color DefaultColor;

        [SerializeField] private HexCell CellPrefab;

        [SerializeField] private HexGridChunk ChunkPrefab;

        private int CellCountX;
        private int CellCountZ;

        private HexCell[] Cells;

        private HexGridChunk[] Chunks;

        private HexCellSignals CellSignals;

        private IHexGridConfig TileConfig;

        private DiContainer Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(HexCellSignals cellSignals, IHexGridConfig tileConfig, DiContainer container) {
            CellSignals = cellSignals;
            TileConfig = tileConfig;
            Container = container;
        }

        #region Unity message methods

        private void Awake() {
            CellCountX = ChunkCountX * HexMetrics.ChunkSizeX;
            CellCountZ = ChunkCountZ * HexMetrics.ChunkSizeZ;

            CreateChunks();
            CreateCells();

            ToggleUI(false);
        }

        #endregion

        #region from IHexGrid        

        public bool HasCellAtCoordinates(HexCoordinates coordinates) {
            int expectedIndex = coordinates.X + coordinates.Z * CellCountX + coordinates.Z / 2;

            return expectedIndex >= 0
                && expectedIndex < Cells.Length
                && Cells[expectedIndex].Coordinates == coordinates;
        }

        public IHexCell GetCellAtCoordinates(HexCoordinates coordinates) {
            int index = coordinates.X + coordinates.Z * CellCountX + coordinates.Z / 2;

            if(index < 0 || index >= Cells.Length) {
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
            throw new NotImplementedException();
        }

        public List<IHexCell> GetCellsInRing(IHexCell center, int radius) {
            throw new NotImplementedException();
        }

        public List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end) {
            return GetShortestPathBetween(start, end, (a, b) => 1);
        }

        public List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end, Func<IHexCell, IHexCell, int> costFunction) {
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
            Dictionary<HexCoordinates, int> costSoFar = new Dictionary<HexCoordinates, int>();
            cameFrom[startCoords] = null;
            costSoFar[startCoords] = 0;

            IHexCell nextHex;
            HexCoordinates current = null;
            while(frontier.Count() > 0) {
                current = frontier.DeleteMin();
                if(current == endCoords) break;

                foreach(var nextCoords in HexCoordinates.GetCoordinatesInRing(current, 1)) {
                    if(!HasCellAtCoordinates(nextCoords)) {
                        continue;
                    }
                    nextHex = GetCellAtCoordinates(nextCoords);

                    int cost = costFunction(GetCellAtCoordinates(current), nextHex);
                    if(cost < 0) {
                        continue;
                    }
                    int newCost = costSoFar[current] + cost;

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
            Cells = new HexCell[CellCountX * CellCountZ];

            for(int z = 0, i = 0; z < CellCountZ; ++ z) {
                for(int x = 0; x < CellCountX; ++x) {
                    CreateCell(x, z, i++);
                }
            }
        }

        private void CreateCell(int x, int z, int i) {
            var position = new Vector3(
                (x + z * 0.5f - z / 2) * HexMetrics.InnerRadius * 2f,
                0f,
                z * HexMetrics.OuterRadius * 1.5f
            );

            var newCell = Cells[i] = Container.InstantiatePrefabForComponent<HexCell>(CellPrefab);

            newCell.transform.localPosition = position;

            newCell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            newCell.Terrain     = TerrainType.Grassland;
            newCell.Shape       = TerrainShape.Flat;
            newCell.Feature     = TerrainFeature.None;
            newCell.Color       = TileConfig.ColorsOfTerrains[(int)newCell.Terrain];
            newCell.Elevation   = 0;

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

        #endregion

    }

}
