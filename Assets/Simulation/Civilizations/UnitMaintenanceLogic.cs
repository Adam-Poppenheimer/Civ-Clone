using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;

namespace Assets.Simulation.Civilizations {

    public class UnitMaintenanceLogic : IUnitMaintenanceLogic {

        #region instance fields and properties

        private IFreeUnitsLogic                               FreeUnitsLogic;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitMaintenanceLogic(
            IFreeUnitsLogic freeUnitsLogic, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ) {
            FreeUnitsLogic      = freeUnitsLogic;
            UnitPossessionCanon = unitPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitMaintenanceLogic

        public float GetMaintenanceOfUnitsForCiv(ICivilization civ) {
            int unitCount = UnitPossessionCanon.GetPossessionsOfOwner(civ).Count();

            int unitsRequiringMaintenance = Math.Max(0, unitCount - FreeUnitsLogic.GetMaintenanceFreeUnitsForCiv(civ));

            return unitsRequiringMaintenance * unitsRequiringMaintenance;
        }

        #endregion

        #endregion

    }

}
