using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public class RegionData {

        #region instance fields and properties

        public IRegionBiomeTemplate    Biome    { get; private set; }
        public IRegionTopologyTemplate Topology { get; private set; }

        private Dictionary<IBalanceStrategy,    int> BalanceStrategyWeights;
        private Dictionary<IResourceDefinition, int> ResourceWeights;




        private IEnumerable<IBalanceStrategy>    AvailableBalanceStrategies;

        #endregion

        #region constructors

        public RegionData(
            IRegionBiomeTemplate biome, IRegionTopologyTemplate topology,
            IEnumerable<IBalanceStrategy> availableBalanceStrategies
        ) {
            Biome                      = biome;
            Topology                   = topology;
            AvailableBalanceStrategies = availableBalanceStrategies;
        }

        #endregion

        #region instance methods

        public Dictionary<IBalanceStrategy, int> GetBalanceStrategyWeights() {
            if(BalanceStrategyWeights == null) {
                BuildBalanceStrategyWeightDictionary();
            }

            return new Dictionary<IBalanceStrategy, int>(BalanceStrategyWeights);
        }

        public Dictionary<IResourceDefinition, int> GetResourceWeights() {
            if(ResourceWeights == null) {
                BuildResourceWeightDictionary();
            }
            
            return new Dictionary<IResourceDefinition, int>(ResourceWeights);
        }

        public int GetWeightOfResource(IResourceDefinition resource) {
            if(ResourceWeights == null) {
                BuildResourceWeightDictionary();
            }

            int retval;
            ResourceWeights.TryGetValue(resource, out retval);
            return retval;
        }

        private void BuildResourceWeightDictionary() {
            ResourceWeights = new Dictionary<IResourceDefinition, int>();

            foreach(var pair in Biome.ResourceWeights.Concat(Topology.ResourceWeights)) {
                if(pair.Resource == null) {
                    continue;
                }

                int weightOfCurrent;
                if(!ResourceWeights.TryGetValue(pair.Resource, out weightOfCurrent)) {
                    ResourceWeights[pair.Resource] = pair.Weight;

                }else if(weightOfCurrent > 0) {
                    ResourceWeights[pair.Resource] += pair.Weight;
                }
            }
        }

        private void BuildBalanceStrategyWeightDictionary() {
            BalanceStrategyWeights = new Dictionary<IBalanceStrategy, int>();

            foreach(var weightPair in Biome.BalanceStrategyWeights.Concat(Topology.BalanceStrategyWeights)) {
                var currentStrategy = AvailableBalanceStrategies.Where(
                    strategy => strategy.Name.Equals(weightPair.BalanceStrategy)
                ).FirstOrDefault();

                if(currentStrategy == null) {
                    continue;
                }

                int weightOfCurrent;
                if(!BalanceStrategyWeights.TryGetValue(currentStrategy, out weightOfCurrent)) {
                    BalanceStrategyWeights[currentStrategy] = weightPair.Weight;

                }else if(weightOfCurrent > 0) {
                    BalanceStrategyWeights[currentStrategy] += weightPair.Weight;
                }
            }
        }

        #endregion

    }

}
