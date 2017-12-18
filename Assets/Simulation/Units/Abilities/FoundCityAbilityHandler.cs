using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Units.Abilities {

    public class FoundCityAbilityHandler : IUnitAbilityHandler {

        #region instance fields and properties

        private ICityValidityLogic CityValidityLogic;

        private IUnitPositionCanon UnitPositionCanon;

        private IRecordkeepingCityFactory CityFactory;

        #endregion

        #region constructors

        [Inject]
        public FoundCityAbilityHandler(ICityValidityLogic cityValidityLogic,
            IUnitPositionCanon unitPositionCanon, IRecordkeepingCityFactory cityFactory
        ){
            CityValidityLogic = cityValidityLogic;
            UnitPositionCanon = unitPositionCanon;
            CityFactory = cityFactory;
        }

        #endregion

        #region instance methods

        #region from IUnitAbilityHandler

        public bool CanHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            if(ability.CommandRequests.Where(request => request.CommandType == AbilityCommandType.FoundCity).Count() != 0) {

                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                return CityValidityLogic.IsTileValidForCity(unitLocation);
            }else {
                return false;
            }
        }

        public bool TryHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            return false;
        }

        #endregion

        #endregion
        
    }

}
