using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Grids;
using UnityCustomUtilities.DataStructures;

namespace Assets.GameMap {

    [ExecuteInEditMode]
    public class MapHexGrid : MonoBehaviour, IMapHexGrid {

        #region instance fields and properties

        #region from IMapHexGrid

        public ReadOnlyCollection<IMapTile> Tiles {
            get {
                if(_tiles == null) {
                    _tiles = new List<IMapTile>();
                    GetComponentsInChildren(_tiles);
                }
                return _tiles.AsReadOnly();
            }
        }
        private List<IMapTile> _tiles;

        private Dictionary<HexCoords, IMapTile> CoordsToHex {
            get {
                if(_coordsToHex == null) {
                    _coordsToHex = new Dictionary<HexCoords, IMapTile>();
                    foreach(var tile in Tiles) {
                        _coordsToHex[tile.Coords] = tile;
                    }
                }
                return _coordsToHex;
            }
        }
        private Dictionary<HexCoords, IMapTile> _coordsToHex;

        [SerializeField] private int Width;
        [SerializeField] private int Height;
        [SerializeField] private HexGridLayout Layout;

        [SerializeField] private GameObject MapTilePrefab;

        #endregion

        #endregion

        #region instance methods

        #region Unity message methods



        #endregion

        #region from IMapHexGrid

        public bool HasTileOfCoords(HexCoords coords) {
            return CoordsToHex.ContainsKey(coords);
        }

        public IMapTile GetTileOfCoords(HexCoords coords) {
            IMapTile retval;
            CoordsToHex.TryGetValue(coords, out retval);
            return retval;
        }

        public bool HasNeighborInDirection(IMapTile center, int neighborDirection) {
            if(center == null) {
                throw new ArgumentNullException("center");
            }
            return HasTileOfCoords(HexCoords.GetNeighborInDirection(center.Coords, neighborDirection));
        }

        public IMapTile GetNeighborInDirection(IMapTile center, int neighborDirection) {
            if(center == null) {
                throw new ArgumentNullException("center");
            }
            return GetTileOfCoords(HexCoords.GetNeighborInDirection(center.Coords, neighborDirection));
        }

        public int GetDistance(IMapTile tileOne, IMapTile tileTwo) {
            if(tileOne == null) {
                throw new ArgumentNullException("tileOne");
            }else if(tileTwo == null) {
                throw new ArgumentNullException("tileTwo");
            }

            return HexCoords.GetDistanceBetween(tileOne.Coords, tileTwo.Coords);
        }

        public List<IMapTile> GetTilesInRadius(IMapTile center, int radius) {
            if(center == null) {
                throw new ArgumentNullException("center");
            }

            var retval = new List<IMapTile>();
            foreach(var coords in HexCoords.GetHexesInRadius(center.Coords, radius)) {
                IMapTile candidateToAdd;
                CoordsToHex.TryGetValue(coords, out candidateToAdd);
                if(candidateToAdd != null) {
                    retval.Add(candidateToAdd);
                }
            }
            return retval;
        }

        public List<IMapTile> GetTilesInLine(IMapTile start, IMapTile end) {
            if(start == null) {
                throw new ArgumentNullException("start");
            }else if(end == null) {
                throw new ArgumentNullException("end");
            }

            var retval = new List<IMapTile>();
            foreach(var coords in FractionalHexCoords.HexLinedraw(start.Coords, end.Coords)) {
                IMapTile candidateToAdd;
                CoordsToHex.TryGetValue(coords, out candidateToAdd);
                if(candidateToAdd != null) {
                    retval.Add(candidateToAdd);
                }
            }
            return retval;
        }

        public List<IMapTile> GetTilesInRing(IMapTile center, int radius) {
            if(center == null) {
                throw new ArgumentNullException("center");
            }

            var retval = new List<IMapTile>();
            foreach(var coords in HexCoords.GetHexRing(center.Coords, radius)) {
                IMapTile candidateToAdd;
                CoordsToHex.TryGetValue(coords, out candidateToAdd);
                if(candidateToAdd != null && candidateToAdd != center) {
                    retval.Add(candidateToAdd);
                }
            }
            return retval;
        }

        public List<IMapTile> GetShortestPathBetween(IMapTile start, IMapTile end) {
            return GetShortestPathBetween(start, end, (a) => 1);
        }

        public List<IMapTile> GetShortestPathBetween(IMapTile start, IMapTile end, Func<IMapTile, int> costFunction) {
            if(start == null) {
                throw new ArgumentNullException("start");
            }else if(end == null) {
                throw new ArgumentNullException("end");
            }

            HexCoords startCoords = start.Coords;
            HexCoords endCoords = end.Coords;
            PriorityQueue<HexCoords> frontier = new PriorityQueue<HexCoords>();
            frontier.Add(startCoords, 0);
            Dictionary<HexCoords, HexCoords> cameFrom = new Dictionary<HexCoords, HexCoords>();
            Dictionary<HexCoords, int> costSoFar = new Dictionary<HexCoords, int>();
            cameFrom[startCoords] = null;
            costSoFar[startCoords] = 0;

            IMapTile nextHex;
            HexCoords current = null;
            while(frontier.Count() > 0) {
                current = frontier.DeleteMin();
                if(current == endCoords) break;

                foreach(var nextCoords in HexCoords.GetHexesInRadius(current, 1)) {
                    if(!CoordsToHex.ContainsKey(nextCoords)) {
                        continue;
                    }
                    nextHex = CoordsToHex[nextCoords];
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
                var results = new List<IMapTile>();
                var lastHex = CoordsToHex[endCoords];
                results.Add(lastHex);
                HexCoords pathAncestor = cameFrom[endCoords];
                while(pathAncestor != startCoords) {
                    var currentHex = CoordsToHex[pathAncestor];
                    results.Add(currentHex);
                    pathAncestor = cameFrom[pathAncestor];
                }
                results.Reverse();
                return results;
            } else {
                return null;
            }
        }

        public List<IMapTile> GetNeighbors(IMapTile center) {
            var retval = new List<IMapTile>();
            for(int i = 0; i < 6; ++i) {
                var neighborCoord = HexCoords.GetNeighborInDirection(center.Coords, i);
                IMapTile neighborToAdd;
                CoordsToHex.TryGetValue(neighborCoord, out neighborToAdd);
                if(neighborToAdd != null) {
                    retval.Add(neighborToAdd);
                }
            }
            return retval;
        }

        #endregion

        public void RegenerateMap() {
            for(int i = transform.childCount - 1; i >= 0; --i) {
                if(Application.isPlaying) {
                    GameObject.Destroy(transform.GetChild(i).gameObject);
                }else {
                    GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }

            for(int r = 0; r < Height; ++r) {
                int rOffset = Mathf.FloorToInt(r / 2f);
                for(int q = -rOffset; q < Width - rOffset; ++q) {
                    ConstructMapTile(q, r);
                }
            }
        }

        private IMapTile ConstructMapTile(int q, int r) {
            var newTileCoords = new HexCoords(q, r, -q - r);

            var newTile = GameObject.Instantiate(MapTilePrefab).GetComponent<MapTile>();
            newTile.Coords = newTileCoords;

            newTile.transform.SetParent(transform, false);

            newTile.transform.localPosition = HexGridLayout.HexCoordsToWorldSpace(Layout, newTileCoords);
            newTile.transform.localScale = (Vector3)Layout.Size + new Vector3(0f, 0f, 1f);

            return newTile;
        }

        #endregion

    }

}


