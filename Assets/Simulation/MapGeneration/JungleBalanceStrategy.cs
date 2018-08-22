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

        public string Name { get { return "Jungle Balance Strategy"; } }

        #endregion

        private int ProductionIncreaseWeight_Hills  = 1000;
        private int ProductionIncreaseWeight_Plains = 1;




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

        public bool TryIncreaseYield(MapRegion region, IRegionTemplate template, YieldType type, out YieldSummary yieldAdded) {
            if(type == YieldType.Production) {
                return TryIncreaseYield_Production(region, out yieldAdded);
            }else {
                yieldAdded = YieldSummary.Empty;
                return false;
            }
        }

        public bool TryIncreaseScore(MapRegion region, IRegionTemplate template, out float scoreAdded) {
            var addJungleCandidates = region.Cells.Where(IncreaseScoreFilter);

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

        public bool TryDecreaseScore(MapRegion region, IRegionTemplate template, out float scoreRemoved) {
            var removeJungleCandidates = region.Cells.Where(DecreaseScoreFilter);

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

        private bool TryIncreaseYield_Production(MapRegion region, out YieldSummary yieldAdded) {
            yieldAdded = YieldSummary.Empty;

            var targetCell = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                region.LandCells, 1, IncreaseProductionWeightFunction
            ).FirstOrDefault();

            if(targetCell != null) {
                var oldYield = YieldEstimator.GetYieldEstimateForCell(targetCell);

                ModLogic.ChangeVegetationOfCell(targetCell, CellVegetation.None);

                var newYield = YieldEstimator.GetYieldEstimateForCell(targetCell);

                yieldAdded = newYield - oldYield;
                return true;
            }else {
                return false;
            }
        }

        private bool IncreaseScoreFilter(IHexCell cell) {
            if(ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.Jungle)) {
                return Grid.GetNeighbors(cell).Count(neighbor => neighbor.Vegetation == CellVegetation.Jungle) >= 3;
            }else {
                return false;
            }
        }

        private bool DecreaseScoreFilter(IHexCell cell) {
            if(cell.Vegetation == CellVegetation.Jungle && ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.None)) {
                return Grid.GetNeighbors(cell).Count(neighbor => neighbor.Vegetation == CellVegetation.None) >= 3;
            }else {
                return false;
            }
        }

        private int IncreaseProductionWeightFunction(IHexCell cell) {
            if(cell.Vegetation != CellVegetation.Jungle || !ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.None)) {
                return 0;
            }else if(cell.Shape == CellShape.Hills) {
                return ProductionIncreaseWeight_Hills;
            }else if(cell.Terrain == CellTerrain.Plains) {
                return ProductionIncreaseWeight_Plains;
            }else {
                return 0;
            }
        }

        #endregion

    }

}
