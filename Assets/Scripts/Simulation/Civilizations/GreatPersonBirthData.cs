using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.Civilizations {

    public struct GreatPersonBirthData {

        public ICivilization   Owner;
        public IUnit           GreatPerson;
        public GreatPersonType Type;

    }

}
