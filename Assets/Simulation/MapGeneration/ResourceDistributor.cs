using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public class ResourceDistributor : IResourceDistributor {

        #region instance fields and properties

        private IMapGenerationConfig      Config;
        private IResourceNodeFactory      NodeFactory;
        private IResourceRestrictionCanon ResourceRestrictionCanon;
        private IStrategicCopiesLogic     StrategicCopiesLogic;

        private IEnumerable<IResourceDefinition> StrategicResources;

        #endregion

        #region constructors

        [Inject]
        public ResourceDistributor(
            IMapGenerationConfig config, IResourceNodeFactory nodeFactory,
            IResourceRestrictionCanon resourceRestrictionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            IStrategicCopiesLogic strategicCopiesLogic,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ) {
            Config                   = config;
            NodeFactory              = nodeFactory;
            ResourceRestrictionCanon = resourceRestrictionCanon;
            StrategicCopiesLogic     = strategicCopiesLogic;

            StrategicResources = availableResources.Where(resource => resource.Type == ResourceType.Strategic);
        }

        #endregion

        #region instance methods

        #region from IResourceDistributor

        public void DistributeStrategicResourcesAcrossRegion(MapRegion region, RegionData regionData) {
            int nodesLeft  = Mathf.CeilToInt(regionData.Resources.StrategicNodesPerCell  * region.Cells.Count);
            int copiesLeft = Mathf.CeilToInt(regionData.Resources.StrategicCopiesPerCell * region.Cells.Count);

            var validStrategics = new List<IResourceDefinition>(StrategicResources);

            Dictionary<IResourceDefinition, int> resourceWeights = regionData.GetResourceWeights();

            while(nodesLeft > 0 && copiesLeft > 0 && validStrategics.Any()) {
                var strategic = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    validStrategics, 1,
                    resource => resourceWeights.ContainsKey(resource) ? resourceWeights[resource] : 0
                ).FirstOrDefault();

                if(strategic == null) {
                    break;
                }

                var location = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                    region.Cells, 1, cell => ResourceRestrictionCanon.GetPlacementWeightOnCell(strategic, cell)
                ).FirstOrDefault();

                if(location == null) {
                    validStrategics.Remove(strategic);

                }else if(NodeFactory.CanBuildNode(location, strategic)) {
                    int copies = StrategicCopiesLogic.GetWeightedRandomCopies();

                    copies = Math.Min(copies, copiesLeft);

                    NodeFactory.BuildNode(location, strategic, copies);

                    nodesLeft--;
                    copiesLeft -= copies;
                }
            }
        }

        public void DistributeResource(
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

        #endregion

        private Func<IHexCell, int> GetResourceWeightFunction(IResourceDefinition resource) {
            return delegate(IHexCell cell) {
                return ResourceRestrictionCanon.GetPlacementWeightOnCell(resource, cell);
            };
        }

        #endregion

    }

}
