using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities {

    public struct CityCaptureData {

        public ICity City;

        public ICivilization OldOwner;
        public ICivilization NewOwner;

    }

}
