using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class HillsBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name { get { return "Hills Balance Strategy"; } }

        #endregion

        private IHexGrid                                         Grid;
        private IYieldEstimator                                  YieldEstimator;
        private IYieldScorer                                     YieldScorer;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private ICellModificationLogic                           ModLogic;

        #endregion

        #region constructors

        [Inject]
        public HillsBalanceStrategy(
            IHexGrid grid, IYieldEstimator yieldEstimator, IYieldScorer yieldScorer,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            ICellModificationLogic modLogic
        ) {
            Grid              = grid;
            YieldEstimator    = yieldEstimator;
            YieldScorer       = yieldScorer;
            NodeLocationCanon = nodeLocationCanon;
            ModLogic          = modLogic;
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(MapRegion region, YieldType type, out YieldSummary yieldAdded) {
            if(type != YieldType.Production) {
                yieldAdded = YieldSummary.Empty;
                return false;
            }

            var candidates = region.Cells.Where(HillCandidateFilter);

            if(candidates.Any()) {
                var newHill = candidates.Random();

                var oldYield = YieldEstimator.GetYieldEstimateForCell(newHill);

                ModLogic.ChangeShapeOfCell(newHill, CellShape.Hills);

                var newYield = YieldEstimator.GetYieldEstimateForCell(newHill);

                yieldAdded = newYield - oldYield;
                return true;
            }else {
                yieldAdded = YieldSummary.Empty;
                return false;
            }
        }

        public bool TryIncreaseScore(MapRegion region, out float scoreAdded) {
            YieldSummary yieldAdded;

            if(TryIncreaseYield(region, YieldType.Production, out yieldAdded)) {
                scoreAdded = YieldScorer.GetScoreOfYield(yieldAdded);
                return true;
            }else {
                scoreAdded = 0f;
                return false;
            }
        }

        public bool TryDecreaseScore(MapRegion region, out float scoreRemoved) {
            scoreRemoved = 0f;
            return false;
        }

        #endregion

        private bool HillCandidateFilter(IHexCell cell) {
            if(NodeLocationCanon.GetPossessionsOfOwner(cell).Any()) {
                return false;
            }

            return cell.Shape == CellShape.Flatlands && cell.Vegetation == CellVegetation.None && (
                cell.Terrain == CellTerrain.Desert || cell.Terrain == CellTerrain.Tundra ||
                cell.Terrain == CellTerrain.Snow
            );
        }

        #endregion

    }

}
