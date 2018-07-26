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

        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public ResourceDistributor(IHexGrid grid) {
            Grid = grid;
        }

        #endregion

        #region instance methods

        #region from IResourceDistributor

        public List<List<IResourceDefinition>> SubdivideResources(
            IEnumerable<IResourceDefinition> resources,
            int groupCount, int resourcesPerGroup
        ) {
            if(resources.Count() < resourcesPerGroup) {
                throw new InvalidOperationException("Not enough resources to create valid subdivisions");
            }

            var retval = new List<List<IResourceDefinition>>();

            var unfinishedSubdivisions = new List<List<IResourceDefinition>>();

            for(int i = 0; i < groupCount; i++) {
                unfinishedSubdivisions.Add(new List<IResourceDefinition>());
            }

            int minUsesOfResource = 0;
            var usesOfResource = new Dictionary<IResourceDefinition, int>();

            foreach(var resource in resources) {
                usesOfResource[resource] = 0;
            }

            int iterations = 10000;
            while(unfinishedSubdivisions.Count > 0 && iterations-- > 0) {
                var subdivision = unfinishedSubdivisions.Random();

                var leastUsedResources = usesOfResource.Keys.Where(
                    resource => usesOfResource[resource] <= minUsesOfResource
                );

                if(leastUsedResources.Any()) {
                    var resource = leastUsedResources.Random();

                    if(!subdivision.Contains(resource)) {
                        subdivision.Add(resource);
                        usesOfResource[resource]++;

                        if(subdivision.Count >= resourcesPerGroup) {
                            retval.Add(subdivision);
                            unfinishedSubdivisions.Remove(subdivision);
                        }
                    }

                }else {
                    minUsesOfResource++;
                }
            }

            return retval;
        }

        public void DistributeResourcesAcrossRegion(
            MapRegion region, IRegionGenerationTemplate template, IEnumerable<IHexCell> oceanCells
        ) {
            throw new NotImplementedException();
        }        

        #endregion

        #endregion

    }

}
