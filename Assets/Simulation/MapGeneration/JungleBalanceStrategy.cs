using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class JungleBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public int SelectionWeight {
            get { return 25; }
        }

        #endregion

        private IHexGrid               Grid;
        private IYieldEstimator        YieldEstimator;
        private ICellModificationLogic ModLogic;
        private IYieldScorer           YieldScorer;

        #endregion

        #region constructors

        [Inject]
        public JungleBalanceStrategy(
            IHexGrid grid, IYieldEstimator yieldEstimator, ICellModificationLogic modLogic,
            IYieldScorer yieldScorer
        ) {
            Grid           = grid;
            YieldEstimator = yieldEstimator;
            ModLogic       = modLogic;
            YieldScorer    = yieldScorer;
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(MapRegion region, YieldType type, out YieldSummary yieldAdded) {
            yieldAdded = YieldSummary.Empty;
            return false;
        }

        public bool TryIncreaseScore(MapRegion region, out float scoreAdded) {
            var addJungleCandidates = region.Cells.Where(AddJungleCandidateFilter);

            if(addJungleCandidates.Any()) {
                var newJungle = addJungleCandidates.Random();

                var oldYield = YieldEstimator.GetYieldEstimateForCell(newJungle);

                ModLogic.ChangeVegetationOfCell(newJungle, CellVegetation.Jungle);

                var newYield = YieldEstimator.GetYieldEstimateForCell(newJungle);

                scoreAdded = YieldScorer.GetScoreOfYield(newYield - oldYield);
                return true;
            }else {
                scoreAdded = 0f;
                return false;
            }
        }

        public bool TryDecreaseScore(MapRegion region, out float scoreRemoved) {
            var removeJungleCandidates = region.Cells.Where(RemoveJungleCandidateFilter);

            if(removeJungleCandidates.Any()) {
                var oldJungle = removeJungleCandidates.Random();

                var oldYield = YieldEstimator.GetYieldEstimateForCell(oldJungle);

                ModLogic.ChangeVegetationOfCell(oldJungle, CellVegetation.None);

                var newYield = YieldEstimator.GetYieldEstimateForCell(oldJungle);

                scoreRemoved = YieldScorer.GetScoreOfYield(newYield - oldYield);
                return true;
            }else {
                scoreRemoved = 0f;
                return false;
            }
        }

        #endregion

        private bool AddJungleCandidateFilter(IHexCell cell) {
            if(ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.Jungle)) {
                return Grid.GetNeighbors(cell).Count(neighbor => neighbor.Vegetation == CellVegetation.Jungle) >= 3;
            }else {
                return false;
            }
        }

        private bool RemoveJungleCandidateFilter(IHexCell cell) {
            if(cell.Vegetation == CellVegetation.Jungle && ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.None)) {
                return Grid.GetNeighbors(cell).Count(neighbor => neighbor.Vegetation == CellVegetation.None) >= 3;
            }else {
                return false;
            }
        }

        #endregion

    }

}
