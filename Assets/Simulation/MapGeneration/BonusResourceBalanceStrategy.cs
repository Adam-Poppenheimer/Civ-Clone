﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class BonusResourceBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name { get { return "Bonus Resource Balance Strategy"; } }

        #endregion

        private IYieldEstimator           YieldEstimator;
        private IResourceNodeFactory      ResourceNodeFactory;
        private IResourceRestrictionCanon ResourceRestrictionCanon;
        private IYieldScorer              YieldScorer;


        private IResourceDefinition[] BonusResources;

        private Dictionary<YieldType, IResourceDefinition[]> BonusResourcesWithYield =
            new Dictionary<YieldType, IResourceDefinition[]>();

        #endregion

        #region constructors

        [Inject]
        public BonusResourceBalanceStrategy(
            IYieldEstimator yieldEstimator, IResourceNodeFactory resourceNodeFactory,
            IResourceRestrictionCanon resourceRestrictionCanon, IYieldScorer yieldScorer,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ) {
            YieldEstimator           = yieldEstimator;
            ResourceNodeFactory      = resourceNodeFactory;
            ResourceRestrictionCanon = resourceRestrictionCanon;
            YieldScorer              = yieldScorer;

            BonusResources = availableResources.Where(
                resource => resource.Type == ResourceType.Bonus
            ).ToArray();

            foreach(var yieldType in EnumUtil.GetValues<YieldType>()) {
                BonusResourcesWithYield[yieldType] = availableResources.Where(
                    resource => resource.Type == ResourceType.Bonus &&
                                YieldEstimator.GetYieldEstimateForResource(resource)[yieldType] > 0f
                ).ToArray();
            }
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(MapRegion region, IRegionTemplate template, YieldType type, out YieldSummary yieldAdded) {
            var availableResources = BonusResourcesWithYield[type].ToList();

            while(availableResources.Count > 0) {
                var chosenResource = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    availableResources, 1, resource => template.GetSelectionWeightOfResource(resource)
                ).FirstOrDefault();

                if(chosenResource == null) {
                    break;
                }

                var cell = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                    region.Cells, 1, GetResourceWeightFunction(chosenResource)
                ).FirstOrDefault();

                if(cell != null) {
                    var oldYield = YieldEstimator.GetYieldEstimateForCell(cell);

                    ResourceNodeFactory.BuildNode(cell, chosenResource, 0);
                    
                    yieldAdded = YieldEstimator.GetYieldEstimateForCell(cell) - oldYield;
                    return true;
                }else {
                    availableResources.Remove(chosenResource);
                }
            }

            yieldAdded = YieldSummary.Empty;
            return false;
        }

        public bool TryIncreaseScore(MapRegion region, IRegionTemplate template, out float scoreAdded) {
            YieldSummary yieldAdded;

            var yieldTypes = EnumUtil.GetValues<YieldType>().ToList();

            while(yieldTypes.Count > 0) {
                var yieldType = yieldTypes.Random();

                if(TryIncreaseYield(region, template, yieldType, out yieldAdded)) {
                    scoreAdded = YieldScorer.GetScoreOfYield(yieldAdded);
                    return true;
                }else {
                    yieldTypes.Remove(yieldType);
                }
            }
            
            scoreAdded = 0f;
            return false;
        }

        public bool TryDecreaseScore(MapRegion region, IRegionTemplate template, out float scoreRemoved) {
            scoreRemoved = 0f;
            return false;
        }

        #endregion

        private Func<IHexCell, int> GetResourceWeightFunction(IResourceDefinition resource) {
            return delegate(IHexCell cell) {
                if(ResourceNodeFactory.CanBuildNode(cell, resource)) {
                    return ResourceRestrictionCanon.GetPlacementWeightOnCell(resource, cell);
                }else {
                    return 0;
                }
            };
        }

        #endregion

    }

}
