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
        private List<IBalanceStrategy> BalanceStrategies;

        #endregion

        #region constructors

        [Inject]
        public RegionBalancer(
            IYieldEstimator cellYieldEstimator, IYieldScorer yieldScorer,
            List<IBalanceStrategy> balanceStrategies
        ) {
            CellYieldEstimator = cellYieldEstimator;
            YieldScorer        = yieldScorer;
            BalanceStrategies  = balanceStrategies;
        }

        #endregion

        #region instance methods

        #region from IRegionBalancer

        public void BalanceRegionYields(
            MapRegion region, IRegionGenerationTemplate template, IEnumerable<IHexCell> oceanCells
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
                YieldType.Food, minFood, region, ref currentYield
            );

            BringYieldTypeToMin(
                YieldType.Production, minProduction, region, ref currentYield
            );


            float minScore = template.MinScorePerCell * region.Cells.Count;
            float maxScore = template.MaxScorePerCell * region.Cells.Count;

            var currentScore = YieldScorer.GetScoreOfYield(currentYield);

            int iterations = 1000;
            float scoreChange;

            while((currentScore < minScore || currentScore > maxScore) && iterations-- > 0) {
                var strategyToAttempt = WeightedRandomSampler<IBalanceStrategy>.SampleElementsFromSet(
                    BalanceStrategies, 1, strategy => strategy.SelectionWeight
                ).FirstOrDefault();

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

            Debug.Log("Post-balance score: " + currentScore);
        }

        #endregion

        private void BringYieldTypeToMin(
            YieldType type, float minYield, MapRegion region,
            ref YieldSummary currentYield
        ) {
            float yieldDeficit = minYield - currentYield[type];

            var validStrategies = new List<IBalanceStrategy>(BalanceStrategies);

            while(yieldDeficit > 0 && validStrategies.Count > 0) {
                var strategy = validStrategies.Random();

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

    }

}
