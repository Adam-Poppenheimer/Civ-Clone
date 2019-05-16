using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface ICivilizationHappinessLogic {

        #region properties

        int GetHappinessOfCiv   (ICivilization civ);
        int GetUnhappinessOfCiv (ICivilization civ);
        int GetNetHappinessOfCiv(ICivilization civ);

        #endregion

    }

}
