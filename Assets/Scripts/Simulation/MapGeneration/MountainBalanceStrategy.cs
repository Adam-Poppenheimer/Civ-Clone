using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public class MountainBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name {
            get { return "Mountain Balance Strategy"; }
        }

        #endregion



        private ICellModificationLogic                           ModLogic;
        private ICellScorer                                      CellScorer;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;        

        #endregion

        #region constructors

        [Inject]
        public MountainBalanceStrategy(
            ICellModificationLogic modLogic, ICellScorer cellScorer,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon
        ) {
            ModLogic          = modLogic;
            CellScorer        = cellScorer;
            NodeLocationCanon = nodeLocationCanon;
            
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(MapRegion region, RegionData regionData, YieldType type, out YieldSummary yieldAdded) {
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

                ModLogic.ChangeShapeOfCell(bestCandidate, CellShape.Mountains);

                scoreRemoved = oldScore - CellScorer.GetScoreOfCell(bestCandidate);
                return true;
            }else {
                scoreRemoved = 0f;
                return false;
            }
        }

        #endregion

        private bool CandidateFilter(IHexCell cell) {
            return cell.Shape == CellShape.Hills
                && ModLogic.CanChangeShapeOfCell(cell, CellShape.Mountains)
                && !NodeLocationCanon.GetPossessionsOfOwner(cell).Any();
        }

        private int CandidateComparer(IHexCell hillOne, IHexCell hillTwo) {
            var hillOneScore = CellScorer.GetScoreOfCell(hillOne);
            var hillTwoScore = CellScorer.GetScoreOfCell(hillTwo);

            return hillTwoScore.CompareTo(hillOneScore);
        }

        #endregion

        
    }

}
