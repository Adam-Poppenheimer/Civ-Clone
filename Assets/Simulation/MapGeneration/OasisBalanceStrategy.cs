using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class OasisBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name {
            get { return "Oasis Balance Strategy"; }
        }

        #endregion

        private int NearbyOasisAvoidance     = 200;
        private int NearbyNonDesertAvoidance = 50;




        private IHexGrid                                         Grid;
        private IYieldEstimator                                  YieldEstimator;
        private ITechCanon                                       TechCanon;
        private IMapScorer                                       MapScorer;
        private ICellModificationLogic                           ModLogic;
        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;
        private IWeightedRandomSampler<IHexCell>                 CellRandomSampler;

        #endregion

        #region constructors

        [Inject]
        public OasisBalanceStrategy(
            IHexGrid grid, IYieldEstimator yieldEstimator, ITechCanon techCanon,
            IMapScorer mapScorer, ICellModificationLogic modLogic,
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            IWeightedRandomSampler<IHexCell> cellRandomSampler
        ) {
            Grid              = grid;
            YieldEstimator    = yieldEstimator;
            TechCanon         = techCanon;
            MapScorer         = mapScorer;
            ModLogic          = modLogic;
            NodePositionCanon = nodePositionCanon;
            CellRandomSampler = cellRandomSampler;
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(
            MapRegion region, RegionData regionData, YieldType type, out YieldSummary yieldAdded
        ) {
            yieldAdded = YieldSummary.Empty;
            if(type != YieldType.Food && type != YieldType.Gold) {                
                return false;
            }

            var newOasis = CellRandomSampler.SampleElementsFromSet(
                region.LandCells, 1, GetOasisCandidateWeightFunction(region)
            ).FirstOrDefault();

            if(newOasis == null) {
                return false;
            }

            var oldYields = new Dictionary<IHexCell, YieldSummary>();

            foreach(var cell in Grid.GetCellsInRadius(newOasis, 1)) {
                oldYields[cell] = YieldEstimator.GetYieldEstimateForCell(cell, TechCanon.AvailableTechs);
            }

            ModLogic.ChangeFeatureOfCell(newOasis, CellFeature.Oasis);

            yieldAdded = YieldSummary.Empty;
            foreach(var cell in oldYields.Keys) {
                yieldAdded += YieldEstimator.GetYieldEstimateForCell(cell, TechCanon.AvailableTechs) - oldYields[cell];
            }

            return true;
        }

        public bool TryIncreaseScore(MapRegion region, RegionData regionData, out float scoreAdded) {
            YieldSummary yieldAdded;

            if(TryIncreaseYield(region, regionData, YieldType.Food, out yieldAdded)) {
                scoreAdded = MapScorer.GetScoreOfYield(yieldAdded);
                return true;
            }else {
                scoreAdded = 0f;
                return false;
            }
        }

        public bool TryDecreaseScore(MapRegion region, RegionData regionData, out float scoreRemoved) {
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
                    neighbor => !neighbor.Terrain.IsWater()
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
