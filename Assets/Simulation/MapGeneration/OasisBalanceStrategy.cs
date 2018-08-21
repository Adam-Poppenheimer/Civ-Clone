using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class OasisBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name {
            get { return "Oasis Balance Strategy"; }
        }

        #endregion

        private int MaxNearbyOasies = 4;

        private int NearbyOasisAvoidance     = 200;
        private int NearbyNonDesertAvoidance = 50;




        private IHexGrid                                         Grid;
        private IYieldEstimator                                  YieldEstimator;
        private IYieldScorer                                     YieldScorer;
        private ICellModificationLogic                           ModLogic;
        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;

        #endregion

        #region constructors

        [Inject]
        public OasisBalanceStrategy(
            IHexGrid grid, IYieldEstimator yieldEstimator,
            IYieldScorer yieldScorer, ICellModificationLogic modLogic,
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon
        ) {
            Grid              = grid;
            YieldEstimator    = yieldEstimator;
            YieldScorer       = yieldScorer;
            ModLogic          = modLogic;
            NodePositionCanon = nodePositionCanon;
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(MapRegion region, YieldType type, out YieldSummary yieldAdded) {
            yieldAdded = YieldSummary.Empty;
            if(type != YieldType.Food && type != YieldType.Gold) {                
                return false;
            }

            var newOasis = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                region.LandCells, 1, GetOasisCandidateWeightFunction(region)
            ).FirstOrDefault();

            if(newOasis == null) {
                return false;
            }

            var oldYields = new Dictionary<IHexCell, YieldSummary>();

            foreach(var cell in Grid.GetCellsInRadius(newOasis, 1)) {
                oldYields[cell] = YieldEstimator.GetYieldEstimateForCell(cell);
            }

            ModLogic.ChangeFeatureOfCell(newOasis, CellFeature.Oasis);

            yieldAdded = YieldSummary.Empty;
            foreach(var cell in oldYields.Keys) {
                yieldAdded += YieldEstimator.GetYieldEstimateForCell(cell) - oldYields[cell];
            }

            return true;
        }

        public bool TryIncreaseScore(MapRegion region, out float scoreAdded) {
            YieldSummary yieldAdded;

            if(TryIncreaseYield(region, YieldType.Food, out yieldAdded)) {
                scoreAdded = YieldScorer.GetScoreOfYield(yieldAdded);
                return true;
            }else {
                scoreAdded = 0f;
                return false;
            }
        }

        public bool TryDecreaseScore(MapRegion region, out float scoreRemoved) {
            scoreRemoved = 0;
            return false;
        }

        #endregion

        private Func<IHexCell, int> GetOasisCandidateWeightFunction(MapRegion region) {
            return delegate(IHexCell cell) {
                if(OasisCandidateFilter(cell, region)) {
                    int nearbyOases     = Grid.GetCellsInRadius(cell, 2).Where(nearby => nearby.Feature == CellFeature.Oasis) .Count();
                    int nearbyNonDesert = Grid.GetCellsInRadius(cell, 2).Where(nearby => nearby.Terrain != CellTerrain.Desert).Count();

                    return Math.Max(1, 1000 - nearbyOases * NearbyOasisAvoidance - nearbyNonDesert * NearbyNonDesertAvoidance);
                }else {
                    return 0;
                }
            };
        }

        private bool OasisCandidateFilter(IHexCell cell, MapRegion region) {
            if(cell.Terrain == CellTerrain.Desert && ModLogic.CanChangeFeatureOfCell(cell, CellFeature.Oasis)) {
                bool surroundedByLand = Grid.GetNeighbors(cell).All(
                    neighbor => region.LandCells.Contains(neighbor) && !neighbor.Terrain.IsWater()
                );

                bool hasResourceNode = NodePositionCanon.GetPossessionsOfOwner(cell).Any();

                return surroundedByLand &&!hasResourceNode;
            }else {
                return false;
            }
        }

        #endregion

    }

}
