using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Abilities {

    public class HurryProductionAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private ICityConfig                                   CityConfig;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public HurryProductionAbilityHandler(
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon, IUnitPositionCanon unitPositionCanon,
            ICityConfig cityConfig, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            CityLocationCanon   = cityLocationCanon;
            UnitPositionCanon   = unitPositionCanon;
            CityConfig          = cityConfig;
            UnitPossessionCanon = unitPossessionCanon;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(!ability.CommandRequests.Any(request => request.CommandType == AbilityCommandType.HurryProduction)) {
                return false;
            }

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var cityAtLocation = CityLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

            if(cityAtLocation == null || cityAtLocation.ActiveProject == null) {
                return false;
            }

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);
            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(cityAtLocation);

            return unitOwner == cityOwner;
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                var cityAt = CityLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

                float productionAdded = CityConfig.HurryAbilityBaseProduction
                                      + CityConfig.HurryAbilityPerPopProduction * cityAt.Population;

                cityAt.ActiveProject.Progress += Mathf.RoundToInt(productionAdded);

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
