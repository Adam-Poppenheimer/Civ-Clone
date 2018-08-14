using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class FreshWaterLakeBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public int SelectionWeight {
            get { return 3; }
        }

        #endregion

        private IHexGrid                                         Grid;
        private IYieldEstimator                                  YieldEstimator;
        private IYieldScorer                                     YieldScorer;
        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;
        private ICellModificationLogic                           ModLogic;

        #endregion

        #region constructors

        [Inject]
        public FreshWaterLakeBalanceStrategy(
            IHexGrid grid, IYieldEstimator yieldEstimator, IYieldScorer yieldScorer,
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            ICellModificationLogic modLogic
        ) {
            Grid              = grid;
            YieldEstimator    = yieldEstimator;
            YieldScorer       = yieldScorer;
            NodePositionCanon = nodePositionCanon;
            ModLogic          = modLogic;
        }

        #endregion

        #region instance methods

        #region IBalanceStrategy

        public bool TryIncreaseYield(MapRegion region, YieldType type, out YieldSummary yieldAdded) {
            if(type != YieldType.Food) {
                yieldAdded = YieldSummary.Empty;
                return false;
            }

            var candidates = region.Cells.Where(GetLakeCandidateFilter(region));

            if(candidates.Any()) {
                var newLake = candidates.Random();

                var oldYields = new Dictionary<IHexCell, YieldSummary>();

                foreach(var cell in Grid.GetCellsInRadius(newLake, 1)) {
                    oldYields[cell] = YieldEstimator.GetYieldEstimateForCell(cell);
                }

                ModLogic.ChangeTerrainOfCell(newLake, CellTerrain.FreshWater);

                yieldAdded = YieldSummary.Empty;
                foreach(var cell in oldYields.Keys) {
                    yieldAdded += YieldEstimator.GetYieldEstimateForCell(cell) - oldYields[cell];
                }

                return true;
            }else {
                yieldAdded = YieldSummary.Empty;
                return false;
            }
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
            scoreRemoved = 0f;
            return false;
        }

        #endregion

        private Func<IHexCell, bool> GetLakeCandidateFilter(MapRegion region) {
            return delegate(IHexCell cell) {
                if( cell.Terrain != CellTerrain.Snow && cell.Terrain != CellTerrain.Desert &&
                    !cell.Terrain.IsWater() && ModLogic.CanChangeTerrainOfCell(cell, CellTerrain.FreshWater)
                ) {
                    bool surroundedByLand = Grid.GetNeighbors(cell).All(
                        neighbor => region.Land.Cells.Contains(neighbor) && neighbor.Terrain != CellTerrain.ShallowWater &&
                                    neighbor.Terrain != CellTerrain.DeepWater
                    );

                    bool hasResourceNode = NodePositionCanon.GetPossessionsOfOwner(cell).Any();

                    bool fewNearbylakes = Grid.GetCellsInRadius(cell, 2).Count(
                        nearby => nearby.Terrain == CellTerrain.FreshWater
                    ) < 3;

                    bool noAdjacentDesert = !Grid.GetNeighbors(cell).Any(neighbor => neighbor.Terrain == CellTerrain.Desert);

                    return surroundedByLand && !hasResourceNode && fewNearbylakes && noAdjacentDesert;
                }else {
                    return false;
                }               
            };
        }

        #endregion

    }

}
