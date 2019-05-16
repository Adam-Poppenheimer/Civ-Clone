using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Civilizations {

    public class FreeUnitsLogic : IFreeUnitsLogic {

        #region instance fields and properties

        private ICivilizationConfig CivConfig;

        #endregion

        #region constructors

        [Inject]
        public FreeUnitsLogic(ICivilizationConfig civConfig) {
            CivConfig = civConfig;
        }

        #endregion

        #region instance methods

        #region from IFreeUnitsLogic

        public int GetMaintenanceFreeUnitsForCiv(ICivilization civ) {
            return CivConfig.MaintenanceFreeUnits;
        }

        #endregion

        #endregion

    }

}
