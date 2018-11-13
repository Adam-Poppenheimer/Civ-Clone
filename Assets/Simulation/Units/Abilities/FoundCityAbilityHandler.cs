using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Abilities {

    public class FoundCityAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private ICityValidityLogic                            CityValidityLogic;
        private IUnitPositionCanon                            UnitPositionCanon;
        private ICityFactory                                  CityFactory;
        private IPossessionRelationship<ICivilization, IUnit> UnitOwnershipCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public FoundCityAbilityHandler(ICityValidityLogic cityValidityLogic,
            IUnitPositionCanon unitPositionCanon, ICityFactory cityFactory,
            IPossessionRelationship<ICivilization, IUnit> unitOwnershipCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ){
            CityValidityLogic   = cityValidityLogic;
            UnitPositionCanon   = unitPositionCanon;
            CityFactory         = cityFactory;
            UnitOwnershipCanon  = unitOwnershipCanon;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitAbilityHandler

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(ability.CommandRequests.Where(request => request.CommandType == AbilityCommandType.FoundCity).Count() != 0) {

                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                return CityValidityLogic.IsCellValidForCity(unitLocation);
            }else {
                return false;
            }
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                var unitOwner = UnitOwnershipCanon.GetOwnerOfPossession(unit);
                var citiesOfOwner = CityPossessionCanon.GetPossessionsOfOwner(unitOwner);

                var cityName = unitOwner.Template.GetNextName(citiesOfOwner);

                CityFactory.Create(UnitPositionCanon.GetOwnerOfPossession(unit), unitOwner, cityName);              

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
