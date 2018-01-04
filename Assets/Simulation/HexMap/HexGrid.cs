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

        #endregion

        [SerializeField] private int Width;
        [SerializeField] private int Height;

        [SerializeField] private Color DefaultColor;

        [SerializeField] private List<Color> ColorsOfTerrain;

        [SerializeField] private HexCell CellPrefab;

        [SerializeField] private Text CellLabelPrefab;

        private Canvas GridCanvas;
        private HexMesh HexMesh;

        private HexCell[] Cells;

        private HexCellSignals CellSignals;

        private DiContainer Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(HexCellSignals cellSignals, DiContainer container) {
            CellSignals = cellSignals;
            Container = container;
        }

        #region Unity message methods

        private void Awake() {
            GridCanvas = GetComponentInChildren<Canvas> ();
            HexMesh    = GetComponentInChildren<HexMesh>();

            Cells = new HexCell[Width * Height];

            for(int z = 0, i = 0; z < Height; ++ z) {
                for(int x = 0; x < Width; ++x) {
                    CreateCell(x, z, i++);
                }
            }
        }

        private void Start() {
            HexMesh.Triangulate(Cells);
        }

        #endregion

        #region from IHexGrid        

        public bool HasCellAtCoordinates(HexCoordinates coordinates) {
            int expectedIndex = coordinates.X + coordinates.Z * Width + coordinates.Z / 2;

            return expectedIndex >= 0
                && expectedIndex < Cells.Length
                && Cells[expectedIndex].Coordinates == coordinates;
        }

        public IHexCell GetCellAtCoordinates(HexCoordinates coordinates) {
            int index = coordinates.X + coordinates.Z * Width + coordinates.Z / 2;

            if(index < 0 || index >= Cells.Length) {
                throw new IndexOutOfRangeException("The given coordinates represent a cell that's not in the grid");
            }

            return Cells[index];
        }

        public bool HasNeighbor(IHexCell center, HexDirection direction) {
            return HasCellAtCoordinates(HexCoordinates.GetNeighborInDirection(center.Coordinates, direction));
        }

        public IHexCell GetNeighbor(IHexCell center, HexDirection direction) {
            return GetCellAtCoordinates(HexCoordinates.GetNeighborInDirection(center.Coordinates, direction));
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
            return GetShortestPathBetween(start, end, (a) => 1);
        }

        public List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end, Func<IHexCell, int> costFunction) {
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
                    if(costFunction(nextHex) < 0) {
                        continue;
                    }
                    int newCost = costSoFar[current] + costFunction(nextHex);

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

        public void PaintCellTerrain(Vector3 position, TerrainType terrain) {
            position = transform.InverseTransformPoint(position);

            var coordinates = HexCoordinates.FromPosition(position);

            var touchedCell = GetCellAtCoordinates(coordinates);

            touchedCell.Terrain = terrain;
            touchedCell.Color = ColorsOfTerrain[(int)terrain];
            HexMesh.Triangulate(Cells);
        }

        public void PaintCellShape(Vector3 position, TerrainShape shape) {
            position = transform.InverseTransformPoint(position);

            var coordinates = HexCoordinates.FromPosition(position);

            var touchedCell = GetCellAtCoordinates(coordinates);

            touchedCell.Shape = shape;
            HexMesh.Triangulate(Cells);
        }

        public void PaintCellFeature(Vector3 position, TerrainFeature feature) {
            position = transform.InverseTransformPoint(position);

            var coordinates = HexCoordinates.FromPosition(position);

            var touchedCell = GetCellAtCoordinates(coordinates);

            touchedCell.Feature = feature;
            HexMesh.Triangulate(Cells);
        }

        #endregion

        private void CreateCell(int x, int z, int i) {
            var position = new Vector3(
                (x + z * 0.5f - z / 2) * HexMetrics.InnerRadius * 2f,
                0f,
                z * HexMetrics.OuterRadius * 1.5f
            );

            var newCell = Cells[i] = Container.InstantiatePrefabForComponent<HexCell>(CellPrefab);
            newCell.transform.SetParent(transform, false);
            newCell.transform.localPosition = position;            

            newCell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            newCell.gameObject.name = string.Format("Cell {0}", newCell.Coordinates);

            newCell.Terrain = TerrainType.Grassland;
            newCell.Shape   = TerrainShape.Flat;
            newCell.Feature = TerrainFeature.None;
            newCell.Color   = ColorsOfTerrain[(int)newCell.Terrain];

            Text label = Instantiate<Text>(CellLabelPrefab);
            label.rectTransform.SetParent(GridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.text = newCell.Coordinates.ToStringOnSeparateLines();
        }

        #endregion

    }

}
