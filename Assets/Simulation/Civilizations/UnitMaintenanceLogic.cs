using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Modifiers;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class UnitMaintenanceLogic : IUnitMaintenanceLogic {

        #region instance fields and properties

        private IFreeUnitsLogic                               FreeUnitsLogic;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ICivModifier<bool>                            SuppressGarrisionMaintenanceModifier;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IUnitPositionCanon                            UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitMaintenanceLogic(
            IFreeUnitsLogic freeUnitsLogic, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            [Inject(Id = "Suppress Garrision Maintenance Modifier")] ICivModifier<bool> suppressGarrisionMaintenanceModifier,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon, IUnitPositionCanon unitPositionCanon
        ) {
            FreeUnitsLogic                       = freeUnitsLogic;
            UnitPossessionCanon                  = unitPossessionCanon;
            SuppressGarrisionMaintenanceModifier = suppressGarrisionMaintenanceModifier;
            CityLocationCanon                    = cityLocationCanon;
            UnitPositionCanon                    = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitMaintenanceLogic

        public float GetMaintenanceOfUnitsForCiv(ICivilization civ) {
            bool suppressGarrisioned = SuppressGarrisionMaintenanceModifier.GetValueForCiv(civ);

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
