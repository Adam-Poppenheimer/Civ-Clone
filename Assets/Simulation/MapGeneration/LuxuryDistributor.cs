using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class LuxuryDistributor : ILuxuryDistributor {

        #region instance fields and properties

        private IResourceDistributor ResourceDistributor;

        #endregion

        #region constructors

        [Inject]
        public LuxuryDistributor(IResourceDistributor resourceDistributor) {
            ResourceDistributor = resourceDistributor;
        }

        #endregion

        #region instance methods

        #region from ILuxuryDistributor

        public void DistributeLuxuriesAcrossHomeland(CivHomelandData homelandData) {
            var luxuriesSelected = new List<IResourceDefinition>();

            var dataToSatisfy = homelandData.LuxuryResources.ToList();
            int iterations = dataToSatisfy.Count * 20;

            while(dataToSatisfy.Any() && iterations-- > 0) {
                var luxuryData = dataToSatisfy.First();

                var luxuryPlacementInRegion = new Dictionary<MapRegion, int>();
                var luxuriesByPriority = new Dictionary<IResourceDefinition, int>();
                int samplesSoFar = 0;

                if(luxuryData.StartingCount > 0) {
                    luxuryPlacementInRegion[homelandData.StartingRegion] = luxuryData.StartingCount;

                    luxuriesByPriority = GetLuxuriesByPriority(
                        homelandData.StartingRegion, homelandData, luxuriesSelected
                    );

                    samplesSoFar++;
                }

                if(luxuryData.OtherCount > 0) {
                    int copiesToAssign = luxuryData.OtherCount;
                    var candidateRegions = homelandData.OtherRegions.ToList();

                    while(copiesToAssign > 0 && candidateRegions.Any()) {

                        MapRegion candidateRegion = candidateRegions.Random();

                        var luxuriesByPriorityWithCandidate = GetCommonLuxuriesByPriority(
                            candidateRegion, luxuriesByPriority, homelandData,
                            samplesSoFar, luxuriesSelected
                        );

                        if(!luxuriesByPriorityWithCandidate.Any()) {
                            candidateRegions.Remove(candidateRegion);
                            continue;
                        }

                        luxuriesByPriority = luxuriesByPriorityWithCandidate;
                        samplesSoFar++;

                        int copiesInRegion = UnityEngine.Random.Range(1, copiesToAssign + 1);
                        if(!luxuryPlacementInRegion.ContainsKey(candidateRegion)) {
                            luxuryPlacementInRegion[candidateRegion] = copiesInRegion;
                        }else {
                            luxuryPlacementInRegion[candidateRegion] += copiesInRegion;
                        }

                        copiesToAssign -= copiesInRegion;
                    }
                }
                var luxuryToPlace = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    luxuriesByPriority.Keys, 1, luxury => luxuriesByPriority[luxury]
                ).FirstOrDefault();

                if(luxuryToPlace == null) {
                    continue;
                }else {
                    dataToSatisfy.Remove(luxuryData);

                    luxuriesSelected.Add(luxuryToPlace);

                    PlaceLuxuryInRegions(luxuryToPlace, luxuryPlacementInRegion, homelandData);
                }
            }

            if(dataToSatisfy.Any()) {
                Debug.LogError("Failed to distribute luxuries correctly across homeland");
            }
        }

        #endregion

        private Dictionary<IResourceDefinition, int> GetLuxuriesByPriority(
            MapRegion region, CivHomelandData homelandData,
            IEnumerable<IResourceDefinition> luxuriesChosen
        ) {
            var regionData = homelandData.GetDataOfRegion(region);

            var resourceWeights = regionData.GetResourceWeights();

            var retval = new Dictionary<IResourceDefinition, int>();

            var validPairs = resourceWeights.Where(
                pair => pair.Key.Type == ResourceType.Luxury && pair.Value > 0 &&
                        !luxuriesChosen.Contains(pair.Key)
            );

            foreach(var resourcePair in validPairs) {
                retval[resourcePair.Key] = resourcePair.Value;
            }

            return retval;
        }

        private Dictionary<IResourceDefinition, int> GetCommonLuxuriesByPriority(
            MapRegion currentRegion, Dictionary<IResourceDefinition, int> existingResourcesWithPriority,
            CivHomelandData homelandData, int samplesSoFar, IEnumerable<IResourceDefinition> luxuriesChosen
        ) {
            var currentLuxuriesByPriority = GetLuxuriesByPriority(currentRegion, homelandData, luxuriesChosen);

            if(!existingResourcesWithPriority.Any()) {
                return currentLuxuriesByPriority;
            }

            var retval = new Dictionary<IResourceDefinition, int>();

            foreach(var existingResource in existingResourcesWithPriority.Keys.Intersect(currentLuxuriesByPriority.Keys)) {
                int previousWeight = existingResourcesWithPriority[existingResource];

                int currentRegionWeight = currentLuxuriesByPriority[existingResource];

                int newWeight = (previousWeight * samplesSoFar + currentRegionWeight) * (samplesSoFar - 1);

                retval[existingResource] = newWeight;
            }

            return retval;
        }

        private void PlaceLuxuryInRegions(
            IResourceDefinition luxury, Dictionary<MapRegion, int> luxuryPlacementInRegion,
            CivHomelandData homelandData
        ) {
            foreach(var region in luxuryPlacementInRegion.Keys) {
                ResourceDistributor.DistributeResource(luxury, region.Cells, luxuryPlacementInRegion[region]);
            }
        }

        #endregion
        
    }

}
