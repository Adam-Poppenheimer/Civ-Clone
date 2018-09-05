using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;
using Assets.Simulation.Technology;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class RegionBalancer : IRegionBalancer {

        #region instance fields and properties

        private IYieldEstimator YieldEstimator;        
        private ICellScorer     CellScorer;
        private ITechCanon      TechCanon;

        #endregion

        #region constructors

        [Inject]
        public RegionBalancer(
            IYieldEstimator yieldEstimator, ICellScorer cellScorer, ITechCanon techCanon
        ) {
            YieldEstimator = yieldEstimator;
            CellScorer     = cellScorer;
            TechCanon      = techCanon;
        }

        #endregion

        #region instance methods

        #region from IRegionBalancer

        public void BalanceRegionYields(MapRegion region, RegionData regionData) {
            if(!regionData.Resources.BalanceResources) {
                return;
            }

            YieldSummary currentYield = YieldSummary.Empty;
            foreach(var cell in region.Cells) {
                currentYield += YieldEstimator.GetYieldEstimateForCell(cell, TechCanon.AvailableTechs);
            }

            float minFood       = regionData.Resources.MinFoodPerCell       * region.Cells.Count;
            float minProduction = regionData.Resources.MinProductionPerCell * region.Cells.Count;

            BringYieldTypeToMin(
                YieldType.Food, minFood, region, regionData, ref currentYield
            );

            BringYieldTypeToMin(
                YieldType.Production, minProduction, region, regionData, ref currentYield
            );


            float minScore = regionData.Resources.MinScorePerCell * region.Cells.Count;
            float maxScore = regionData.Resources.MaxScorePerCell * region.Cells.Count;

            var currentScore = region.Cells.Select(cell => CellScorer.GetScoreOfCell(cell)).Sum();

            int iterations = 1000;
            float scoreChange;

            Dictionary<IBalanceStrategy, int> strategyWeights = regionData.GetBalanceStrategyWeights();

            while((currentScore < minScore || currentScore > maxScore) && iterations-- > 0) {
                var strategyToAttempt = GetStrategy(strategyWeights);

                if(strategyToAttempt == null) {
                    break;
                }

                if(currentScore < minScore && strategyToAttempt.TryIncreaseScore(region, regionData, out scoreChange)) {
                    currentScore += scoreChange;
                }

                if(currentScore > maxScore && strategyToAttempt.TryDecreaseScore(region, regionData, out scoreChange)) {
                    currentScore -= scoreChange;
                }
            }
        }

        #endregion

        private void BringYieldTypeToMin(
            YieldType type, float minYield, MapRegion region, RegionData regionData,
            ref YieldSummary currentYield
        ) {
            float yieldDeficit = minYield - currentYield[type];

            Dictionary<IBalanceStrategy, int> strategyWeights = regionData.GetBalanceStrategyWeights();

            while(yieldDeficit > 0 && strategyWeights.Count > 0) {
                var strategy = GetStrategy(strategyWeights);

                if(strategy == null) {
                    Debug.LogWarningFormat("Failed to bring yield type {0} to min yield {1}", type, minYield);
                    break;
                }

                YieldSummary yieldAdded;
                if(strategy.TryIncreaseYield(region, regionData, type, out yieldAdded)) {
                    yieldDeficit -= yieldAdded[type];
                    currentYield += yieldAdded;
                }else {
                    strategyWeights.Remove(strategy);
                }
            }
        }

        private IBalanceStrategy GetStrategy(Dictionary<IBalanceStrategy, int> strategyWeights) {
            return WeightedRandomSampler<IBalanceStrategy>.SampleElementsFromSet(
                strategyWeights.Keys, 1, strategy => strategyWeights[strategy]
            ).FirstOrDefault();
        }

        private float GetScorePerCell(MapRegion region) {
            var cellsByScore = region.Cells.Select(cell => CellScorer.GetScoreOfCell(cell));

            return cellsByScore.Aggregate((current, next) => current + next) / region.Cells.Count;
        }

        #endregion

    }

}
