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

    public class ResourceBalanceStrategy : IBalanceStrategy {

        #region instance fields and properties

        #region from IBalanceStrategy

        public string Name { get { return "Resource Balance Strategy"; } }

        #endregion

        private Dictionary<YieldType, IResourceDefinition[]> BonusResourcesWithYield =
            new Dictionary<YieldType, IResourceDefinition[]>();

        private List<IResourceDefinition> ScoreIncreasingCandidates;



        private IYieldEstimator           YieldEstimator;
        private IResourceNodeFactory      ResourceNodeFactory;
        private IResourceRestrictionCanon ResourceRestrictionCanon;
        private ICellScorer               CellScorer;
        private ITechCanon                TechCanon;
        private IStrategicCopiesLogic     StrategicCopiesLogic;

        #endregion

        #region constructors

        [Inject]
        public ResourceBalanceStrategy(
            IYieldEstimator yieldEstimator, IResourceNodeFactory resourceNodeFactory,
            IResourceRestrictionCanon resourceRestrictionCanon, ICellScorer cellScorer, ITechCanon techCanon,
            IStrategicCopiesLogic strategicCopiesLogic,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ) {
            YieldEstimator           = yieldEstimator;
            ResourceNodeFactory      = resourceNodeFactory;
            ResourceRestrictionCanon = resourceRestrictionCanon;
            CellScorer               = cellScorer;
            TechCanon                = techCanon;
            StrategicCopiesLogic     = strategicCopiesLogic;

            foreach(var yieldType in EnumUtil.GetValues<YieldType>()) {
                BonusResourcesWithYield[yieldType] = availableResources.Where(
                    resource => resource.Type == ResourceType.Bonus &&
                                YieldEstimator.GetYieldEstimateForResource(resource)[yieldType] > 0f
                ).ToArray();
            }

            ScoreIncreasingCandidates = availableResources.Where(resource => resource.Type != ResourceType.Luxury).ToList();
        }

        #endregion

        #region instance methods

        #region from IBalanceStrategy

        public bool TryIncreaseYield(
            MapRegion region, RegionData regionData, YieldType type, out YieldSummary yieldAdded
        ) {
            var availableResources = BonusResourcesWithYield[type].ToList();

            while(availableResources.Count > 0) {
                var chosenResource = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    availableResources, 1, GetResourceSelectionWeightFunction(region, regionData)
                ).FirstOrDefault();

                if(chosenResource == null) {
                    break;
                }

                var cell = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                    region.Cells, 1, GetCellPlacementWeightFunction(chosenResource)
                ).FirstOrDefault();

                if(cell != null) {
                    var oldYield = YieldEstimator.GetYieldEstimateForCell(cell, TechCanon.AvailableTechs);

                    int copies = chosenResource.Type == ResourceType.Strategic
                               ? StrategicCopiesLogic.GetWeightedRandomCopies()
                               : 0;

                    ResourceNodeFactory.BuildNode(cell, chosenResource, copies);
                    
                    yieldAdded = YieldEstimator.GetYieldEstimateForCell(cell, TechCanon.AvailableTechs) - oldYield;
                    return true;
                }else {
                    availableResources.Remove(chosenResource);
                }
            }

            yieldAdded = YieldSummary.Empty;
            return false;
        }

        public bool TryIncreaseScore(
            MapRegion region, RegionData regionData, out float scoreAdded
        ) {
            var candidateResources = new List<IResourceDefinition>(ScoreIncreasingCandidates);

            while(candidateResources.Any()) {
                var chosenResource = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    candidateResources, 1, GetResourceSelectionWeightFunction(region, regionData)
                ).FirstOrDefault();

                if(chosenResource == null) {
                    break;
                }

                var cell = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                    region.Cells, 1, GetCellPlacementWeightFunction(chosenResource)
                ).FirstOrDefault();

                if(cell != null) {
                    float oldScore = CellScorer.GetScoreOfCell(cell);

                    int copies = chosenResource.Type == ResourceType.Strategic
                               ? StrategicCopiesLogic.GetWeightedRandomCopies()
                               : 0;

                    ResourceNodeFactory.BuildNode(cell, chosenResource, copies);
                    
                    scoreAdded = CellScorer.GetScoreOfCell(cell) - oldScore;
                    return true;
                }else {
                    candidateResources.Remove(chosenResource);
                }
            }

            scoreAdded = 0f;
            return false;
        }

        public bool TryDecreaseScore(
            MapRegion region, RegionData regionData, out float scoreRemoved
        ) {
            scoreRemoved = 0f;
            return false;
        }

        #endregion

        private Func<IResourceDefinition, int> GetResourceSelectionWeightFunction(
            MapRegion region, RegionData regionData
        ) {
            return delegate(IResourceDefinition resource) {
                if(region.Cells.Any(cell => ResourceRestrictionCanon.IsResourceValidOnCell(resource, cell))) {
                    return regionData.GetWeightOfResource(resource);
                }else {
                    return 0;
                }
            };
        }

        private Func<IHexCell, int> GetCellPlacementWeightFunction(IResourceDefinition resource) {
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
