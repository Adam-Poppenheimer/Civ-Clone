using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class ResourceSampler : IResourceSampler {

        #region instance fields and properties

        private IResourceNodeFactory NodeFactory;

        private IEnumerable<IResourceDefinition> AvailableLuxuries;

        private List<IResourceDefinition> LuxuriesLittleUsed = new List<IResourceDefinition>();

        #endregion

        #region constructors

        public ResourceSampler(
            IResourceNodeFactory nodeFactory,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ) {
            NodeFactory       = nodeFactory;

            AvailableLuxuries = availableResources.Where(resource => resource.Type == ResourceType.Luxury);
        }

        #endregion

        #region instance methods

        public void Reset() {
            LuxuriesLittleUsed.Clear();
        }

        #region from IResourceSampler

        public ResourceSamplerResults GetLuxuryForRegion(
            MapRegion region, int nodesNeeded,
            List<IResourceDefinition> luxuriesSoFar
        ) {
            if(LuxuriesLittleUsed.Count == 0) {
                LuxuriesLittleUsed.AddRange(AvailableLuxuries);
            }

            var validLuxuries = new List<IResourceDefinition>();

            var cellsForLuxury = new Dictionary<IResourceDefinition, List<IHexCell>>();

            foreach(var luxury in AvailableLuxuries) {
                var validCells = region.Cells.Where(cell => IsCellValidForLuxury(cell, luxury)).ToList();

                if(validCells.Count >= nodesNeeded && !luxuriesSoFar.Contains(luxury)) {
                    validLuxuries.Add(luxury);

                    cellsForLuxury[luxury] = validCells;
                }
            }

            var validLuxuriesLittleUsed = validLuxuries.Intersect(LuxuriesLittleUsed);

            if(validLuxuriesLittleUsed.Any()) {
                var sampledLuxury = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    validLuxuriesLittleUsed, 1, luxury => cellsForLuxury[luxury].Count
                ).First();

                LuxuriesLittleUsed.Remove(sampledLuxury);

                return new ResourceSamplerResults(sampledLuxury, cellsForLuxury[sampledLuxury]);

            }else if(validLuxuries.Any()) {
                var sampledLuxury = WeightedRandomSampler<IResourceDefinition>.SampleElementsFromSet(
                    validLuxuries, 1, luxury => cellsForLuxury[luxury].Count
                ).First();

                return new ResourceSamplerResults(sampledLuxury, cellsForLuxury[sampledLuxury]);

            }else {
                throw new InvalidOperationException(
                    "The argued region cannot support the argued number of nodes for any available luxury resource"
                );
            }
        }

        #endregion

        private bool IsCellValidForLuxury(IHexCell cell, IResourceDefinition resource) {
            return NodeFactory.CanBuildNode(cell, resource);
        }

        #endregion

    }

}
