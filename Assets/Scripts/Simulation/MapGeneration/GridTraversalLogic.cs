using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.DataStructures;

namespace Assets.Simulation.MapGeneration {

    public class GridTraversalLogic : IGridTraversalLogic {

        #region instance fields and properties

        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public GridTraversalLogic(IHexGrid grid) {
            Grid   = grid;
        }

        #endregion

        #region instance methods

        #region from IGridTraversalLogic

        public IEnumerator<IHexCell> GetCrawlingEnumerator(
            IHexCell seed, IEnumerable<IHexCell> availableCells, IEnumerable<IHexCell> acceptedCells,
            CrawlingWeightFunction weightFunction
        ) {
            var searchFrontier = new PriorityQueue<IHexCell>();

            searchFrontier.Add(seed, 0);

            while(searchFrontier.Count() > 0) {
                IHexCell current = searchFrontier.DeleteMin();

                if(availableCells.Contains(current)) {
                    yield return current;
                    ExpandFrontier(current, availableCells, searchFrontier, acceptedCells, seed, weightFunction);
                }
            }
        }

        #endregion

        private void ExpandFrontier(
            IHexCell center, IEnumerable<IHexCell> availableCells, PriorityQueue<IHexCell> searchFrontier,
            IEnumerable<IHexCell> acceptedCells, IHexCell seed, CrawlingWeightFunction weightFunction
        ) {
            foreach(var neighbor in Grid.GetNeighbors(center)) {

                if(availableCells.Contains(neighbor) && !searchFrontier.Contains(neighbor)) {
                    int candidateWeight = weightFunction(neighbor, seed, acceptedCells);

                    if(candidateWeight >= 0) {
                        searchFrontier.Add(neighbor, candidateWeight);
                    }
                }
            }
        }

        #endregion

    }

}
