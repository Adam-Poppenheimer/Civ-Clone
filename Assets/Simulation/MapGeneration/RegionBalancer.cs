using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class RegionBalancer : IRegionBalancer {

        #region instance fields and properties

        private IYieldEstimator        CellYieldEstimator;        
        private IYieldScorer           YieldScorer;
        private List<IBalanceStrategy> AllBalanceStrategies;

        #endregion

        #region constructors

        [Inject]
        public RegionBalancer(
            IYieldEstimator cellYieldEstimator, IYieldScorer yieldScorer,
            List<IBalanceStrategy> allBalanceStrategies
        ) {
            CellYieldEstimator   = cellYieldEstimator;
            YieldScorer          = yieldScorer;
            AllBalanceStrategies = allBalanceStrategies;
        }

        #endregion

        #region instance methods

        #region from IRegionBalancer

        public void BalanceRegionYields(
            MapRegion region, IRegionTemplate template
        ) {
            if(!template.BalanceResources) {
                return;
            }

            YieldSummary currentYield = YieldSummary.Empty;
            foreach(var cell in region.Cells) {
                currentYield += CellYieldEstimator.GetYieldEstimateForCell(cell);
            }

            float minFood       = template.MinFoodPerCell       * region.Cells.Count;
            float minProduction = template.MinProductionPerCell * region.Cells.Count;

            BringYieldTypeToMin(
                YieldType.Food, minFood, region, template, ref currentYield
            );

            BringYieldTypeToMin(
                YieldType.Production, minProduction, region, template, ref currentYield
            );


            float minScore = template.MinScorePerCell * region.Cells.Count;
            float maxScore = template.MaxScorePerCell * region.Cells.Count;

            var currentScore = YieldScorer.GetScoreOfYield(currentYield);

            int iterations = 1000;
            float scoreChange;

            while((currentScore < minScore || currentScore > maxScore) && iterations-- > 0) {
                var strategyToAttempt = GetStrategy(AllBalanceStrategies, template);

                if(strategyToAttempt == null) {
                    break;
                }

                if(currentScore < minScore && strategyToAttempt.TryIncreaseScore(region, out scoreChange)) {
                    currentScore += scoreChange;
                }

                if(currentScore > maxScore && strategyToAttempt.TryDecreaseScore(region, out scoreChange)) {
                    currentScore -= scoreChange;
                }
            }

            currentYield = YieldSummary.Empty;
            foreach(var cell in region.Cells) {
                currentYield += CellYieldEstimator.GetYieldEstimateForCell(cell);
            }

            currentScore = YieldScorer.GetScoreOfYield(currentYield);
        }

        #endregion

        private void BringYieldTypeToMin(
            YieldType type, float minYield, MapRegion region, IRegionTemplate template,
            ref YieldSummary currentYield
        ) {
            float yieldDeficit = minYield - currentYield[type];

            var validStrategies = new List<IBalanceStrategy>(AllBalanceStrategies);

            while(yieldDeficit > 0 && validStrategies.Count > 0) {
                var strategy = GetStrategy(validStrategies, template);

                if(strategy == null) {
                    Debug.LogWarningFormat("Failed to bring yield type {0} to min yield {1}", type, minYield);
                    break;
                }

                YieldSummary yieldAdded;
                if(strategy.TryIncreaseYield(region, type, out yieldAdded)) {
                    yieldDeficit -= yieldAdded[type];
                    currentYield += yieldAdded;
                }else {
                    validStrategies.Remove(strategy);
                }
            }
        }

        #endregion

        private IBalanceStrategy GetStrategy(IEnumerable<IBalanceStrategy> strategies, IRegionTemplate template) {
            return WeightedRandomSampler<IBalanceStrategy>.SampleElementsFromSet(
                strategies, 1, strategy => template.GetWeightForBalanceStrategy(strategy)
            ).FirstOrDefault();
        }

    }

}
