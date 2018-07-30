using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public class ResourceSamplerResults {

        #region instance fields and properties

        public readonly IResourceDefinition Resource;

        public readonly IEnumerable<IHexCell> ValidLocations;

        #endregion

        #region constructors

        [Inject]
        public ResourceSamplerResults(IResourceDefinition resource, IEnumerable<IHexCell> validLocations) {
            Resource       = resource;
            ValidLocations = validLocations;
        }

        #endregion

    }

}
