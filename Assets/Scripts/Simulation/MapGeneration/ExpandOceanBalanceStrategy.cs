using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public class ExpandOceanBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name {
            get { return "Expand Ocean Balance Strategy"; }
        }

        #endregion



        private ICellModificationLogic                           ModLogic;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private ICellScorer                                      CellScorer;
        private IRiverCanon                                      RiverCanon;
        private IHexGrid                                         Grid;

        #endregion

        #region constructors

        [Inject]
        public ExpandOceanBalanceStrategy(
            ICellModificationLogic modLogic, IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            ICellScorer cellScorer, IRiverCanon riverCanon, IHexGrid grid
        ) {
            ModLogic          = modLogic;
            NodeLocationCanon = nodeLocationCanon;
            CellScorer        = cellScorer;
            RiverCanon        = riverCanon;
            Grid              = grid;
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(
            MapRegion region, RegionData regionData, YieldType type, out YieldSummary yieldAdded
        ) {
            yieldAdded = YieldSummary.Empty;
            return false;
        }

        public bool TryIncreaseScore(MapRegion region, RegionData regionData, out float scoreAdded) {
            scoreAdded = 0f;
            return false;
        }

        public bool TryDecreaseScore(MapRegion region, RegionData regionData, out float scoreRemoved) {
            var allCandidates = region.LandCells.Where(CandidateFilter).ToList();

            allCandidates.Sort(CandidateComparer);

            var bestCandidate = allCandidates.FirstOrDefault();

            if(bestCandidate != null) {
                var oldScore = CellScorer.GetScoreOfCell(bestCandidate);

                ModLogic.ChangeTerrainOfCell(bestCandidate, CellTerrain.ShallowWater);

                scoreRemoved = oldScore - CellScorer.GetScoreOfCell(bestCandidate);

                return true;
            }else {
                scoreRemoved = 0f;
                return false;
            }
        }

        #endregion

        private bool CandidateFilter(IHexCell cell) {
            return !RiverCanon.HasRiver(cell)
                && !NodeLocationCanon.GetPossessionsOfOwner(cell).Any()
                && ModLogic.CanChangeTerrainOfCell(cell, CellTerrain.ShallowWater)
                && Grid.GetNeighbors(cell).Any(neighbor => neighbor.Terrain == CellTerrain.ShallowWater)
                && !Grid.GetNeighbors(cell).Any(neighbor => neighbor.Terrain == CellTerrain.FreshWater);
        }

        private int CandidateComparer(IHexCell cellOne, IHexCell cellTwo) {
            var cellOneScore = CellScorer.GetScoreOfCell(cellOne);
            var cellTwoScore = CellScorer.GetScoreOfCell(cellTwo);

            return cellTwoScore.CompareTo(cellOneScore);
        }

        #endregion

    }

}
