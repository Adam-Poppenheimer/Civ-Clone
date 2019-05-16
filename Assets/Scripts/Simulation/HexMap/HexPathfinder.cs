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
            return GetShortestPathBetween(start, end, float.MaxValue, costFunction, availableCells);
        }

        //Note: Since units can technically overspend their movement on the last cell
        //they move into, this implementation allows for paths longer than maxCost,
        //but only if that cost is exceeded on the last cell reached. It does this by
        //adding such cells to the costSoFar dictionary but excluding them from the
        //frontier
        public List<IHexCell> GetShortestPathBetween(
            IHexCell start, IHexCell end, float maxCost, Func<IHexCell, IHexCell, float> costFunction,
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

                        if(newCost < maxCost) {
                            float heuristic = HexCoordinates.GetDistanceBetween(nextCell.Coordinates, end.Coordinates);
                            frontier.Add(nextCell, newCost + heuristic);
                        }
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
            return GetAllCellsReachableIn(start, float.MaxValue, costFunction, availableCells);
        }

        //Note: Since units can technically overspend their movement on the last cell
        //they move into, this implementation allows for paths longer than maxCost,
        //but only if that cost is exceeded on the last cell reached. It does this by
        //adding such cells to the costSoFar dictionary but excluding them from the
        //frontier
        public Dictionary<IHexCell, float> GetAllCellsReachableIn(
            IHexCell start, float maxCost, Func<IHexCell, IHexCell, float> costFunction,
            IEnumerable<IHexCell> availableCells
        ) {
            if(start == null) {
                throw new ArgumentNullException("start");

            }else if(costFunction == null) {
                throw new ArgumentNullException("costFunction");

            } else if(!availableCells.Contains(start)) {
                throw new InvalidOperationException("start must be within availableCells");

            }else if(maxCost <= 0) {
                throw new ArgumentOutOfRangeException("maxCost", "Must be >= 0");
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

                        if(newCost < maxCost) {
                            frontier.Add(nextCell, newCost);
                        }
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
