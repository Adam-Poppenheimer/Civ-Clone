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

        #endregion

        #region constructors

        [Inject]
        public ResourceDistributor(
            IMapGenerationConfig config, IResourceNodeFactory nodeFactory,
            IResourceSampler resourceSampler, IResourceRestrictionCanon resourceRestrictionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon, IHexGrid grid
        ) {
            Config                   = config;
            NodeFactory              = nodeFactory;
            ResourceSampler          = resourceSampler;
            ResourceRestrictionCanon = resourceRestrictionCanon;
            Grid                     = grid;
        }

        #endregion

        #region instance methods

        #region from IResourceDistributor

        public void DistributeLuxuryResourcesAcrossRegion(
            MapRegion region, IRegionGenerationTemplate template, IEnumerable<IHexCell> oceanCells
        ) {
            var resourcesSoFar = new List<IResourceDefinition>();

            if(template.HasPrimaryLuxury) {
                DistributeLuxury(
                    region, oceanCells, template.PrimaryLuxuryCount, resourcesSoFar
                );
            }

            if(template.HasSecondaryLuxury) {
                DistributeLuxury(
                    region, oceanCells, template.SecondaryLuxuryCount, resourcesSoFar
                );
            }

            if(template.HasTertiaryLuxury) {
                DistributeLuxury(
                    region, oceanCells, template.TertiaryLuxuryCount, resourcesSoFar
                );
            }

            if(template.HasQuaternaryLuxury) {
                DistributeLuxury(
                    region, oceanCells, template.QuaternaryLuxuryCount, resourcesSoFar
                );
            }
        }

        #endregion

        private void DistributeLuxury(
            MapRegion region, IEnumerable<IHexCell> oceanCells,
            int nodeCount, List<IResourceDefinition> luxuriesSoFar
        ) {
            var results = ResourceSampler.GetLuxuryForRegion(region, nodeCount, luxuriesSoFar);

            luxuriesSoFar.Add(results.Resource);

            DistributeResource(results.Resource, results.ValidLocations, nodeCount, oceanCells);
        }

        private void DistributeResource(
            IResourceDefinition resource, IEnumerable<IHexCell> validLocations,
            int count, IEnumerable<IHexCell> oceanCells
        ) {
            if(validLocations.Count() < count) {
                Debug.LogWarning("Could not find enough valid locations for resoure " + resource.name);
            }

            var nodeLocations = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                validLocations, count, GetResourceWeightFunction(resource, validLocations)
            );

            foreach(var location in nodeLocations) {
                int copies = resource.Type == ResourceType.Strategic
                           ? UnityEngine.Random.Range(Config.MinStrategicCopies, Config.MaxStrategicCopies) : 1;

                NodeFactory.BuildNode(location, resource, copies);
            }
        }

        private Func<IHexCell, int> GetResourceWeightFunction(IResourceDefinition resource, IEnumerable<IHexCell> validLocations) {
            return delegate(IHexCell cell) {
                return ResourceRestrictionCanon.GetPlacementWeightOnCell(resource, cell);
            };
        }

        #endregion

    }

}
