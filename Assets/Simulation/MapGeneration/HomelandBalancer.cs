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

    public class HomelandBalancer : IHomelandBalancer {

        #region instance fields and properties

        private IYieldEstimator YieldEstimator;        
        private ICellScorer     CellScorer;
        private ITechCanon      TechCanon;

        #endregion

        #region constructors

        [Inject]
        public HomelandBalancer(
            IYieldEstimator yieldEstimator, ICellScorer cellScorer, ITechCanon techCanon
        ) {
            YieldEstimator = yieldEstimator;
            CellScorer     = cellScorer;
            TechCanon      = techCanon;
        }

        #endregion

        #region instance methods

        #region from IRegionBalancer

        public void BalanceHomelandYields(HomelandData homelandData) {
            YieldSummary currentYield = YieldSummary.Empty;
            foreach(var cell in homelandData.Cells) {
                currentYield += YieldEstimator.GetYieldEstimateForCell(cell, TechCanon.AvailableTechs);
            }

            float minFood       = homelandData.YieldAndResources.MinFoodPerCell       * homelandData.Cells.Count();
            float minProduction = homelandData.YieldAndResources.MinProductionPerCell * homelandData.Cells.Count();

            BringYieldTypeToMin(
                YieldType.Food, minFood, homelandData, ref currentYield
            );

            BringYieldTypeToMin(
                YieldType.Production, minProduction, homelandData, ref currentYield
            );

            float minScore = homelandData.YieldAndResources.MinScorePerCell * homelandData.Cells.Count();
            float maxScore = homelandData.YieldAndResources.MaxScorePerCell * homelandData.Cells.Count();

            KeepScoreWithinBounds(homelandData, minScore, maxScore);

            float minScorePerCell = minScore / homelandData.Cells.Count();
            float maxScorePerCell = maxScore / homelandData.Cells.Count();
            float currentScorePerCell = homelandData.Cells.Average(cell => CellScorer.GetScoreOfCell(cell));

            if(currentScorePerCell < minScorePerCell) {
                Debug.LogFormat(
                    "A homeland has a score per cell of {0}, from a minimum of {1}",
                    currentScorePerCell, minScorePerCell
                );

                foreach(var cell in homelandData.Cells) {
                    cell.SetMapData(0f);
                }
            }else if(currentScorePerCell > maxScorePerCell) {
                Debug.LogFormat(
                    "A homeland has a score per cell of {0}, from a maximum of {1}",
                    currentScorePerCell, maxScorePerCell
                );

                foreach(var cell in homelandData.Cells) {
                    cell.SetMapData(1f);
                }
            }else {
                foreach(var cell in homelandData.Cells) {
                    cell.SetMapData(0.5f);
                }
            }
        }

        #endregion

        private void BringYieldTypeToMin(
            YieldType type, float minYield, HomelandData homelandData,
            ref YieldSummary currentYield
        ) {
            float yieldDeficit = minYield - currentYield[type];

            var strategyWeightsByRegion = new Dictionary<MapRegion, Dictionary<IBalanceStrategy, int>>();

            foreach(var region in homelandData.AllRegions) {
                strategyWeightsByRegion[region] = homelandData.GetDataOfRegion(region).GetBalanceStrategyWeights();
            }

            int iterations = homelandData.AllRegions.Count() * 10;

            List<MapRegion> regions = homelandData.AllRegions.ToList();

            while(yieldDeficit > 0 && iterations-- > 0) {
                if(regions.Count == 0) {
                    regions = homelandData.AllRegions.ToList();
                }

                var region = regions.Random();
                regions.Remove(region);

                var strategyWeights = strategyWeightsByRegion[region];

                var strategy = GetStrategy(strategyWeights);

                YieldSummary yieldAdded;
                if(strategy.TryIncreaseYield(region, homelandData.GetDataOfRegion(region), type, out yieldAdded)) {
                    yieldDeficit -= yieldAdded[type];
                    currentYield += yieldAdded;
                }
            }

            if(currentYield[type] < minYield) {
                Debug.LogWarningFormat("Failed to bring yield type {0} to min yield {1}", type, minYield);
            }
        }

        private void KeepScoreWithinBounds(HomelandData homelandData, float minScore, float maxScore) {
            var currentScore = homelandData.Cells.Select(cell => CellScorer.GetScoreOfCell(cell)).Sum();
            
            float scoreChange;

            var strategyWeightsByRegion = new Dictionary<MapRegion, Dictionary<IBalanceStrategy, int>>();

            foreach(var region in homelandData.AllRegions) {
                strategyWeightsByRegion[region] = homelandData.GetDataOfRegion(region).GetBalanceStrategyWeights();
            }

            int iterations = homelandData.AllRegions.Count() * 50;

            var regions = homelandData.AllRegions.ToList();

            while((currentScore < minScore || currentScore > maxScore) && iterations-- > 0) {
                if(regions.Count == 0) {
                    regions.AddRange(homelandData.AllRegions);
                }

                var region = regions.Random();
                regions.Remove(region);

                var regionData = homelandData.GetDataOfRegion(region);

                var strategyToAttempt = GetStrategy(strategyWeightsByRegion[region]);

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
