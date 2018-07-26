using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.Civilizations {

    public struct ResourceTransfer {

        public readonly ICivilization Exporter;
        public readonly ICivilization Importer;

        public readonly IResourceDefinition Resource;

        public readonly int Copies;

        public ResourceTransfer(
            ICivilization exporter, ICivilization importer, IResourceDefinition resource, int copies
        ){
            Exporter = exporter;
            Importer = importer;
            Resource = resource;
            Copies   = copies;
        }

    }

}
