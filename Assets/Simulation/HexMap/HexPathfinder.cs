using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using UnityCustomUtilities.DataStructures;

namespace Assets.Simulation.HexMap {

    public class HexPathfinder : IHexPathfinder {

        #region instance fields and properties

        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public HexPathfinder(IHexGrid grid) {
            Grid = grid;
        }

        #endregion

        #region instance methods

        #region from IHexPathfinder

        public List<IHexCell> GetShortestPathBetween(
            IHexCell start, IHexCell end, Func<IHexCell, IHexCell, float> costFunction
        ) {
            return GetShortestPathBetween(start, end, costFunction, Grid.Cells);
        }

        public List<IHexCell> GetShortestPathBetween(
            IHexCell start, IHexCell end, Func<IHexCell, IHexCell, float> costFunction,
            IEnumerable<IHexCell> availableCells
        ) {
            if(start == null) {
                throw new ArgumentNullException("start");

            }else if(end == null) {
                throw new ArgumentNullException("end");

            }else if(!availableCells.Contains(start)) {
                throw new InvalidOperationException("start must be within availableCells");

            }else if(!availableCells.Contains(end)) {
                throw new InvalidOperationException("end must be within availableCells");
            }

            if(start == end) {
                return null;
            }

            PriorityQueue<IHexCell> frontier = new PriorityQueue<IHexCell>();
            frontier.Add(start, 0);

            Dictionary<IHexCell, IHexCell> cameFrom  = new Dictionary<IHexCell, IHexCell>();
            Dictionary<IHexCell, float>    costSoFar = new Dictionary<IHexCell, float>();

            cameFrom[start] = null;
            costSoFar[start] = 0;

            IHexCell currentCell = null;
                        
            while(frontier.Count() > 0) {
                currentCell = frontier.DeleteMin();
                if(currentCell.Equals(end)) break;

                foreach(var nextCell in GetAdjacentCells(currentCell, availableCells)) {
                    float cost = costFunction(currentCell, nextCell);
                    if(cost < 0) {
                        continue;
                    }

                    float newCost = costSoFar[currentCell] + cost;

                    if(!costSoFar.ContainsKey(nextCell) || newCost < costSoFar[nextCell]) {
                        costSoFar[nextCell] = newCost;
                        cameFrom[nextCell] = currentCell;

                        float heuristic = HexCoordinates.GetDistanceBetween(nextCell.Coordinates, end.Coordinates);
                        frontier.Add(nextCell, newCost + heuristic);
                    }
                }
            }

            if(cameFrom.ContainsKey(end)) {
                var results = new List<IHexCell>();
                results.Add(end);

                IHexCell pathAncestor = cameFrom[end];

                while(pathAncestor != start) {
                    results.Add(pathAncestor);
                    pathAncestor = cameFrom[pathAncestor];
                }
                results.Reverse();
                return results;
            } else {
                return null;
            }
        }

        public Dictionary<IHexCell, float> GetCostToAllCells(
            IHexCell start, Func<IHexCell, IHexCell, float> costFunction,
            IEnumerable<IHexCell> availableCells
        ) {
            if(start == null) {
                throw new ArgumentNullException("start");

            }else if(costFunction == null) {
                throw new ArgumentNullException("costFunction");

            } else if(!availableCells.Contains(start)) {
                throw new InvalidOperationException("start must be within availableCells");

            }

            PriorityQueue<IHexCell> frontier = new PriorityQueue<IHexCell>();
            frontier.Add(start, 0);

            Dictionary<IHexCell, IHexCell> cameFrom  = new Dictionary<IHexCell, IHexCell>();
            Dictionary<IHexCell, float>    costSoFar = new Dictionary<IHexCell, float>();

            cameFrom[start] = null;
            costSoFar[start] = 0;

            IHexCell currentCell = null;
                        
            while(frontier.Count() > 0) {
                currentCell = frontier.DeleteMin();

                foreach(var nextCell in GetAdjacentCells(currentCell, availableCells)) {
                    float cost = costFunction(currentCell, nextCell);
                    if(cost < 0) {
                        continue;
                    }

                    float newCost = costSoFar[currentCell] + cost;

                    if(!costSoFar.ContainsKey(nextCell) || newCost < costSoFar[nextCell]) {
                        costSoFar[nextCell] = newCost;
                        cameFrom[nextCell] = currentCell;

                        frontier.Add(nextCell, newCost);
                    }
                }
            }

            return costSoFar;
        }

        #endregion

        private IEnumerable<IHexCell> GetAdjacentCells(IHexCell centeredCell, IEnumerable<IHexCell> availableCells) {
            return Grid.GetNeighbors(centeredCell).Intersect(availableCells);
        }

        #endregion

    }

}
