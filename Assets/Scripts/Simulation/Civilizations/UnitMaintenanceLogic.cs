using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class UnitMaintenanceLogic : IUnitMaintenanceLogic {

        #region instance fields and properties

        private IFreeUnitsLogic                               FreeUnitsLogic;
        private ICivModifiers                                 CivModifiers;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitGarrisonLogic                            UnitGarrisonLogic;

        #endregion

        #region constructors

        [Inject]
        public UnitMaintenanceLogic(
            IFreeUnitsLogic freeUnitsLogic, ICivModifiers civModifiers,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitGarrisonLogic unitGarrisonLogic
        ) {
            FreeUnitsLogic      = freeUnitsLogic;
            CivModifiers        = civModifiers;
            UnitPossessionCanon = unitPossessionCanon;
            UnitGarrisonLogic   = unitGarrisonLogic;
        }

        #endregion

        #region instance methods

        #region from IUnitMaintenanceLogic

        public float GetMaintenanceOfUnitsForCiv(ICivilization civ) {
            bool suppressGarrisioned = CivModifiers.SuppressGarrisonMaintenance.GetValueForCiv(civ);

            int countRequiringMaintenance = 0;

            foreach(IUnit candidate in UnitPossessionCanon.GetPossessionsOfOwner(civ).Where(unit => unit.Type != UnitType.City)) {
                if(!suppressGarrisioned || !UnitGarrisonLogic.IsUnitGarrisoned(candidate)) {
                    countRequiringMaintenance++;
                }
            }

            int unitsRequiringMaintenance = Math.Max(
                0, countRequiringMaintenance - FreeUnitsLogic.GetMaintenanceFreeUnitsForCiv(civ)
            );

            return unitsRequiringMaintenance * unitsRequiringMaintenance;
        }

        #endregion

        #endregion

    }

}
