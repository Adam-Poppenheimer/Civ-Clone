using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Civilizations {

    public struct ResourceTransfer {

        public readonly ICivilization Exporter;
        public readonly ICivilization Importer;

        public readonly ISpecialtyResourceDefinition Resource;

        public readonly int Copies;

        public ResourceTransfer(
            ICivilization exporter, ICivilization importer, ISpecialtyResourceDefinition resource, int copies
        ){
            Exporter = exporter;
            Importer = importer;
            Resource = resource;
            Copies   = copies;
        }

    }

}
