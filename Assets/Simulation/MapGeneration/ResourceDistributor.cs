using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class ResourceDistributor : IResourceDistributor {

        #region instance fields and properties

        private IMapGenerationConfig      Config;
        private IResourceNodeFactory      NodeFactory;
        private IResourceSampler          ResourceSampler;
        private IResourceRestrictionCanon ResourceRestrictionCanon;
        private IHexGrid                  Grid;
        private IStrategicCopiesLogic     StrategicCopiesLogic;

        private IEnumerable<IResourceDefinition> StrategicResources;

        #endregion

        #region constructors

        [Inject]
        public ResourceDistributor(
            IMapGenerationConfig config, IResourceNodeFactory nodeFactory,
            IResourceSampler resourceSampler, IResourceRestrictionCanon resourceRestrictionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon, IHexGrid grid,
            IStrategicCopiesLogic strategicCopiesLogic,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ) {
            Config                   = config;
            NodeFactory              = nodeFactory;
            ResourceSampler          = resourceSampler;
            ResourceRestrictionCanon = resourceRestrictionCanon;
            Grid                     = grid;
            StrategicCopiesLogic     = strategicCopiesLogic;

            StrategicResources = availableResources.Where(resource => resource.Type == ResourceType.Strategic);
        }

        #endregion

        #region instance methods

        #region from IResourceDistributor

        public void DistributeLuxuryResourcesAcrossRegion(
            MapRegion region, IRegionGenerationTemplate template
        ) {
            var resourcesSoFar = new List<IResourceDefinition>();

            if(template.HasPrimaryLuxury) {
                DistributeLuxury(
                    region, template.PrimaryLuxuryCount, resourcesSoFar
                );
            }

            if(template.HasSecondaryLuxury) {
                DistributeLuxury(
                    region, template.SecondaryLuxuryCount, resourcesSoFar
                );
            }

            if(template.HasTertiaryLuxury) {
                DistributeLuxury(
                    region, template.TertiaryLuxuryCount, resourcesSoFar
                );
            }

            if(template.HasQuaternaryLuxury) {
                DistributeLuxury(
                    region, template.QuaternaryLuxuryCount, resourcesSoFar
                );
            }
        }

        public void DistributeStrategicResourcesAcrossRegion(
            MapRegion region, IRegionGenerationTemplate template
        ) {
            int nodesLeft  = Mathf.CeilToInt(template.StrategicNodesPerCell  * region.Cells.Count);
            int copiesLeft = Mathf.CeilToInt(template.StrategicCopiesPerCell * region.Cells.Count);

            var validStrategics = new List<IResourceDefinition>(StrategicResources);

            while(nodesLeft > 0 && copiesLeft > 0 && validStrategics.Any()) {
                var strategic = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    validStrategics, 1, resource => resource.SelectionWeight
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

        #endregion

        private void DistributeLuxury(
            MapRegion region, int nodeCount, List<IResourceDefinition> luxuriesSoFar
        ) {
            var results = ResourceSampler.GetLuxuryForRegion(region, nodeCount, luxuriesSoFar);

            luxuriesSoFar.Add(results.Resource);

            DistributeResource(results.Resource, results.ValidLocations, nodeCount);
        }

        private void DistributeResource(
            IResourceDefinition resource, IEnumerable<IHexCell> validLocations,
            int count
        ) {
            if(validLocations.Count() < count) {
                Debug.LogWarning("Could not find enough valid locations for resoure " + resource.name);
            }

            var nodeLocations = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                validLocations, count, GetResourceWeightFunction(resource)
            );

            foreach(var location in nodeLocations) {
                int copies = resource.Type == ResourceType.Strategic
                           ? UnityEngine.Random.Range(Config.MinStrategicCopies, Config.MaxStrategicCopies) : 1;

                NodeFactory.BuildNode(location, resource, copies);
            }
        }

        private Func<IHexCell, int> GetResourceWeightFunction(IResourceDefinition resource) {
            return delegate(IHexCell cell) {
                return ResourceRestrictionCanon.GetPlacementWeightOnCell(resource, cell);
            };
        }

        #endregion

    }

}
