﻿using System;
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
            List<IResourceDefinition> luxuriesForStarting = GetLuxuriesForStartingRegion(homelandData);
            List<IResourceDefinition> luxuriesForOthers   = GetLuxuriesForOtherRegions  (homelandData);
            List<IResourceDefinition> luxuriesForWhole    = GetLuxuriesForWholeHomeland (homelandData);

            var luxuriesAlreadyChosen = new HashSet<IResourceDefinition>();

            foreach(var luxuryData in homelandData.LuxuryResources) {
                if(luxuryData.ConstrainedToStarting) {
                    DistributeLuxuryAcrossSingleRegion(
                        homelandData.StartingRegion, luxuryData.StartingCount,
                        luxuriesForStarting, luxuriesAlreadyChosen
                    );
                }else if(luxuryData.ConstrainedToOthers) {
                    DistributeLuxuryAcrossMultipleRegions(
                        homelandData.OtherRegions, luxuryData.OtherCount,
                        luxuriesForOthers, luxuriesAlreadyChosen
                    );
                }else {
                    if(!TryDistributeLuxuryAcrossSingleAndMultipleRegions(
                        homelandData.StartingRegion, homelandData.OtherRegions,
                        luxuryData.StartingCount, luxuryData.OtherCount,
                        luxuriesForWhole, luxuriesAlreadyChosen
                    )) {
                        DistributeLuxuryAcrossSingleRegion(
                            homelandData.StartingRegion, luxuryData.StartingCount,
                            luxuriesForStarting, luxuriesAlreadyChosen
                        );

                        DistributeLuxuryAcrossMultipleRegions(
                            homelandData.OtherRegions, luxuryData.OtherCount,
                            luxuriesForOthers, luxuriesAlreadyChosen
                        );
                    }
                }
            }
        }

        #endregion

        private void DistributeLuxuryAcrossSingleRegion(
            MapRegion region, int nodeCount, List<IResourceDefinition> validLuxuries,
            HashSet<IResourceDefinition> luxuriesAlreadyChosen
        ) {
            while(validLuxuries.Any()) {
                var candidate = validLuxuries.Last();
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
            foreach(var cell in region.Cells) {
                cell.SetMapData(0.75f);
            }
        }

        private void DistributeLuxuryAcrossMultipleRegions(
            IEnumerable<MapRegion> regions, int nodeCount, List<IResourceDefinition> validLuxuries,
            HashSet<IResourceDefinition> luxuriesAlreadyChosen
        ) {
            while(validLuxuries.Any()) {
                var candidate = validLuxuries.Last();
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
            foreach(var cell in regions.SelectMany(region => region.Cells)) {
                cell.SetMapData(0.75f);
            }
        }

        private bool TryDistributeLuxuryAcrossSingleAndMultipleRegions(
            MapRegion singleRegion, IEnumerable<MapRegion> multipleRegions, int singleNodeCount,
            int multipleNodeCount, List<IResourceDefinition> validLuxuries,
            HashSet<IResourceDefinition> luxuriesAlreadyChosen
        ) {
            while(validLuxuries.Any()) {
                var candidate = validLuxuries.Last();
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


        private List<IResourceDefinition> GetLuxuriesForStartingRegion(HomelandData homelandData) {
            var priorityOfResources = homelandData.StartingData.GetResourceWeights();

            var retval = priorityOfResources.Keys.Where(resource => resource.Type == ResourceType.Luxury).ToList();

            retval.Sort((first, second) => priorityOfResources[first].CompareTo(priorityOfResources[second]));

            return retval;
        }

        private List<IResourceDefinition> GetLuxuriesForOtherRegions(HomelandData homelandData) {
            var globalPriorityOfLuxuries = MashTogetherPriorityDictionaries(homelandData, homelandData.OtherRegions);

            var retval = globalPriorityOfLuxuries.Keys.ToList();

            retval.Sort((first, second) => globalPriorityOfLuxuries[first].CompareTo(globalPriorityOfLuxuries[second]));

            return retval;
        }

        private List<IResourceDefinition> GetLuxuriesForWholeHomeland(HomelandData homelandData) {
            var globalPriorityOfLuxuries = MashTogetherPriorityDictionaries(homelandData, homelandData.AllRegions);

            var retval = globalPriorityOfLuxuries.Keys.ToList();

            retval.Sort((first, second) => globalPriorityOfLuxuries[first].CompareTo(globalPriorityOfLuxuries[second]));

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
