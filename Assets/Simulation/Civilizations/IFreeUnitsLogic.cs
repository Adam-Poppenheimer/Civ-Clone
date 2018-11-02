using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface IFreeUnitsLogic {

        #region methods

        int GetMaintenanceFreeUnitsForCiv(ICivilization civ);

        #endregion

    }

}
