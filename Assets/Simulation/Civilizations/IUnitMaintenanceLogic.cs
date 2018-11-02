using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface IUnitMaintenanceLogic {

        #region methods

        float GetMaintenanceOfUnitsForCiv(ICivilization civ);

        #endregion

    }

}
