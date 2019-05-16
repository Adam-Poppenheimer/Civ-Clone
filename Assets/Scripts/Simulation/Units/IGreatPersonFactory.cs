using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public interface IGreatPersonFactory {

        #region methods
        bool  CanBuildGreatPerson(GreatPersonType type, ICivilization owner);
        IUnit BuildGreatPerson   (GreatPersonType type, ICivilization owner);

        #endregion

    }

}
