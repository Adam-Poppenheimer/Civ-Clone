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
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IUnitPositionCanon                            UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitMaintenanceLogic(
            IFreeUnitsLogic freeUnitsLogic, ICivModifiers civModifiers,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IUnitPositionCanon unitPositionCanon
        ) {
            FreeUnitsLogic      = freeUnitsLogic;
            CivModifiers        = civModifiers;
            UnitPossessionCanon = unitPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
            UnitPositionCanon   = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitMaintenanceLogic

        public float GetMaintenanceOfUnitsForCiv(ICivilization civ) {
            bool suppressGarrisioned = CivModifiers.SuppressGarrisonMaintenance.GetValueForCiv(civ);

            int countRequiringMaintenance = 0;

            foreach(IUnit candidate in UnitPossessionCanon.GetPossessionsOfOwner(civ).Where(unit => unit.Type != UnitType.City)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(candidate);

                var citiesAtLocation = CityLocationCanon.GetPossessionsOfOwner(unitLocation);

                if(!suppressGarrisioned || !citiesAtLocation.Any()) {
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
