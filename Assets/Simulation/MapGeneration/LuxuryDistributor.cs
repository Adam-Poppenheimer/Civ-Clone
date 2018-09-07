using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class LuxuryDistributor : ILuxuryDistributor {

        #region instance fields and properties

        private IResourceRestrictionCanon ResourceRestrictionCanon;
        private IMapGenerationConfig      Config;
        private IResourceNodeFactory      NodeFactory;

        #endregion

        #region constructors

        [Inject]
        public LuxuryDistributor(
            IResourceRestrictionCanon resourceRestrictionCanon, IMapGenerationConfig config,
            IResourceNodeFactory nodeFactory
        ) {
            ResourceRestrictionCanon = resourceRestrictionCanon;
            Config                   = config;
            NodeFactory              = nodeFactory;
        }

        #endregion

        #region instance methods

        #region from ILuxuryDistributor

        public void DistributeLuxuriesAcrossHomeland(HomelandData homelandData) {
            Dictionary<IResourceDefinition, int> weightForStarting;
            Dictionary<IResourceDefinition, int> weightForOthers;
            Dictionary<IResourceDefinition, int> weightForWhole;

            List<IResourceDefinition> luxuriesForStarting = GetLuxuriesForStartingRegion(homelandData, out weightForStarting);
            List<IResourceDefinition> luxuriesForOthers   = GetLuxuriesForOtherRegions  (homelandData, out weightForOthers);
            List<IResourceDefinition> luxuriesForWhole    = GetLuxuriesForWholeHomeland (homelandData, out weightForWhole);

            var luxuriesAlreadyChosen = new HashSet<IResourceDefinition>();

            foreach(var luxuryData in homelandData.LuxuryResources) {
                if(luxuryData.ConstrainedToStarting) {
                    DistributeLuxuryAcrossSingleRegion(
                        homelandData.StartingRegion, luxuryData.StartingCount,
                        luxuriesForStarting, weightForStarting, luxuriesAlreadyChosen
                    );
                }else if(luxuryData.ConstrainedToOthers) {
                    DistributeLuxuryAcrossMultipleRegions(
                        homelandData.OtherRegions, luxuryData.OtherCount,
                        luxuriesForOthers, weightForOthers, luxuriesAlreadyChosen
                    );
                }else {
                    if(!TryDistributeLuxuryAcrossSingleAndMultipleRegions(
                        homelandData.StartingRegion, homelandData.OtherRegions,
                        luxuryData.StartingCount, luxuryData.OtherCount,
                        luxuriesForWhole, weightForWhole, luxuriesAlreadyChosen
                    )) {
                        DistributeLuxuryAcrossSingleRegion(
                            homelandData.StartingRegion, luxuryData.StartingCount,
                            luxuriesForStarting, weightForStarting, luxuriesAlreadyChosen
                        );

                        DistributeLuxuryAcrossMultipleRegions(
                            homelandData.OtherRegions, luxuryData.OtherCount,
                            luxuriesForOthers, weightForOthers, luxuriesAlreadyChosen
                        );
                    }
                }
            }
        }

        #endregion

        private void DistributeLuxuryAcrossSingleRegion(
            MapRegion region, int nodeCount, List<IResourceDefinition> validLuxuries,
            Dictionary<IResourceDefinition, int> weightForResources,
            HashSet<IResourceDefinition> luxuriesAlreadyChosen
        ) {
            while(validLuxuries.Any()) {
                var candidate = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    validLuxuries, 1, luxury => weightForResources[luxury]
                ).FirstOrDefault();
                validLuxuries.Remove(candidate);

                if(luxuriesAlreadyChosen.Contains(candidate)) {
                    continue;
                }

                var validCells = region.Cells.Where(
                    cell => ResourceRestrictionCanon.IsResourceValidOnCell(candidate, cell)
                );

                if(validCells.Count() >= nodeCount) {
                    DistributeResource(candidate, validCells, nodeCount);
                    luxuriesAlreadyChosen.Add(candidate);
                    return;
                }
            }

            Debug.LogWarning("Failed to perform luxury distribution on region");
        }

        private void DistributeLuxuryAcrossMultipleRegions(
            IEnumerable<MapRegion> regions, int nodeCount, List<IResourceDefinition> validLuxuries,            
            Dictionary<IResourceDefinition, int> weightForResources,
            HashSet<IResourceDefinition> luxuriesAlreadyChosen
        ) {
            while(validLuxuries.Any()) {
                var candidate = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    validLuxuries, 1, luxury => weightForResources[luxury]
                ).FirstOrDefault();
                validLuxuries.Remove(candidate);

                if(luxuriesAlreadyChosen.Contains(candidate)) {
                    continue;
                }

                var validCells = regions.SelectMany(region => region.Cells).Where(
                    cell => ResourceRestrictionCanon.IsResourceValidOnCell(candidate, cell)
                );

                if(validCells.Count() >= nodeCount) {
                    DistributeResource(candidate, validCells, nodeCount);
                    luxuriesAlreadyChosen.Add(candidate);
                    return;
                }
            }

            Debug.LogWarning("Failed to perform luxury distribution across multiple regions");
        }

        private bool TryDistributeLuxuryAcrossSingleAndMultipleRegions(
            MapRegion singleRegion, IEnumerable<MapRegion> multipleRegions, int singleNodeCount,
            int multipleNodeCount, List<IResourceDefinition> validLuxuries,
            Dictionary<IResourceDefinition, int> weightForResources,
            HashSet<IResourceDefinition> luxuriesAlreadyChosen
        ) {
            while(validLuxuries.Any()) {
                var candidate = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    validLuxuries, 1, luxury => weightForResources[luxury]
                ).FirstOrDefault();
                validLuxuries.Remove(candidate);

                if(luxuriesAlreadyChosen.Contains(candidate)) {
                    continue;
                }

                var validSingleCells = singleRegion.Cells.Where(
                    cell => ResourceRestrictionCanon.IsResourceValidOnCell(candidate, cell)
                );

                var validMultipleCells = multipleRegions.SelectMany(region => region.Cells).Where(
                    cell => ResourceRestrictionCanon.IsResourceValidOnCell(candidate, cell)
                );

                if(validSingleCells.Count() >= singleNodeCount && validMultipleCells.Count() >= multipleNodeCount) {
                    DistributeResource(candidate, validSingleCells,   singleNodeCount);
                    DistributeResource(candidate, validMultipleCells, multipleNodeCount);
                    luxuriesAlreadyChosen.Add(candidate);
                    return true;
                }
            }

            return false;
        }

        private void DistributeResource(
            IResourceDefinition resource, IEnumerable<IHexCell> validLocations,
            int count
        ) {
            var nodeLocations = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                validLocations, count, GetResourceWeightFunction(resource)
            );

            if(nodeLocations.Count < count) {
                Debug.LogWarningFormat(
                    "Could not find enough valid locations to place {0} {1} after weighting. Only found {2}",
                    count, resource.name, nodeLocations.Count
                );
            }

            foreach(var location in nodeLocations) {
                int copies = resource.Type == ResourceType.Strategic
                           ? UnityEngine.Random.Range(Config.MinStrategicCopies, Config.MaxStrategicCopies) : 1;

                NodeFactory.BuildNode(location, resource, copies);
            }
        }


        private List<IResourceDefinition> GetLuxuriesForStartingRegion(
            HomelandData homelandData, out Dictionary<IResourceDefinition, int> weightOfResources
        ) {
            weightOfResources = homelandData.StartingData.GetResourceWeights();

            var retval = weightOfResources.Keys.ToList();

            return retval;
        }

        private List<IResourceDefinition> GetLuxuriesForOtherRegions(
            HomelandData homelandData, out Dictionary<IResourceDefinition, int> weightOfResources
        ) {
            weightOfResources = MashTogetherPriorityDictionaries(homelandData, homelandData.OtherRegions);

            var retval = weightOfResources.Keys.ToList();

            return retval;
        }

        private List<IResourceDefinition> GetLuxuriesForWholeHomeland(
            HomelandData homelandData, out Dictionary<IResourceDefinition, int> weightOfResources
        ) {
            weightOfResources = MashTogetherPriorityDictionaries(homelandData, homelandData.AllRegions);

            var retval = weightOfResources.Keys.ToList();

            return retval;
        }

        private Dictionary<IResourceDefinition, int> MashTogetherPriorityDictionaries(
            HomelandData homelandData, IEnumerable<MapRegion> allRegions
        ) {
            var allRegionData = allRegions.Select(region => homelandData.GetDataOfRegion(region));

            var mashedTogetherPriorities = new Dictionary<IResourceDefinition, int>();

            foreach(var regionData in allRegionData) {
                var resourceWeights = regionData.GetResourceWeights();

                foreach(var luxury in resourceWeights.Keys.Where(resource => resource.Type == ResourceType.Luxury)) {
                    int globalPriority;

                    mashedTogetherPriorities.TryGetValue(luxury, out globalPriority);

                    globalPriority += resourceWeights[luxury];

                    mashedTogetherPriorities[luxury] = globalPriority;
                }
            }

            return mashedTogetherPriorities;
        }

        private Func<IHexCell, int> GetResourceWeightFunction(IResourceDefinition resource) {
            return delegate(IHexCell cell) {
                return ResourceRestrictionCanon.GetPlacementWeightOnCell(resource, cell);
            };
        }

        #endregion
        
    }

}
