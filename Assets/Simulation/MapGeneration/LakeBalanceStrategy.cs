using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class LakeBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name { get { return "Lake Balance Strategy"; } }

        #endregion

        private int MaxNearbyLakes = 3;




        private IHexGrid                                         Grid;
        private IYieldEstimator                                  YieldEstimator;
        private IYieldScorer                                     YieldScorer;
        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;
        private ICellModificationLogic                           ModLogic;

        #endregion

        #region constructors

        [Inject]
        public LakeBalanceStrategy(
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

        public bool TryIncreaseYield(
            MapRegion region, RegionData regionData, YieldType type, out YieldSummary yieldAdded
        ) {
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

        public bool TryIncreaseScore(MapRegion region, RegionData regionData, out float scoreAdded) {
            YieldSummary yieldAdded;

            if(TryIncreaseYield(region, regionData, YieldType.Food, out yieldAdded)) {
                scoreAdded = YieldScorer.GetScoreOfYield(yieldAdded);
                return true;
            }else {
                scoreAdded = 0f;
                return false;
            }
        }

        public bool TryDecreaseScore(MapRegion region, RegionData regionData, out float scoreRemoved) {
            scoreRemoved = 0f;
            return false;
        }

        #endregion

        private Func<IHexCell, bool> GetLakeCandidateFilter(MapRegion region) {
            return delegate(IHexCell cell) {
                if( cell.Terrain != CellTerrain.Snow && cell.Terrain != CellTerrain.Desert &&
                    !cell.Terrain.IsWater() && ModLogic.CanChangeTerrainOfCell(cell, CellTerrain.FreshWater)
                ) {
                    bool surroundedByLandOrLakes = Grid.GetNeighbors(cell).All(
                        neighbor => neighbor.Terrain != CellTerrain.ShallowWater &&
                                    neighbor.Terrain != CellTerrain.DeepWater
                    );

                    bool hasResourceNode = NodePositionCanon.GetPossessionsOfOwner(cell).Any();

                    bool fewNearbylakes = Grid.GetCellsInRadius(cell, 2).Count(
                        nearby => nearby.Terrain == CellTerrain.FreshWater
                    ) < MaxNearbyLakes;

                    bool noAdjacentDesert = !Grid.GetNeighbors(cell).Any(neighbor => neighbor.Terrain == CellTerrain.Desert);

                    return surroundedByLandOrLakes && !hasResourceNode && fewNearbylakes && noAdjacentDesert;
                }else {
                    return false;
                }               
            };
        }

        #endregion

    }

}
