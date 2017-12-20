using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Abilities {

    public class FoundCityAbilityHandler : IUnitAbilityHandler {

        #region instance fields and properties

        private ICityValidityLogic CityValidityLogic;

        private IUnitPositionCanon UnitPositionCanon;

        private ICityFactory CityFactory;

        private IPossessionRelationship<ICivilization, IUnit> UnitOwnershipCanon;

        #endregion

        #region constructors

        [Inject]
        public FoundCityAbilityHandler(ICityValidityLogic cityValidityLogic,
            IUnitPositionCanon unitPositionCanon, ICityFactory cityFactory,
            IPossessionRelationship<ICivilization, IUnit> unitOwnershipCanon
        ){
            CityValidityLogic  = cityValidityLogic;
            UnitPositionCanon  = unitPositionCanon;
            CityFactory        = cityFactory;
            UnitOwnershipCanon = unitOwnershipCanon;
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

        public AbilityExecutionResults TryHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                CityFactory.Create(UnitPositionCanon.GetOwnerOfPossession(unit), UnitOwnershipCanon.GetOwnerOfPossession(unit));

                if(Application.isPlaying) {
                    GameObject.Destroy(unit.gameObject);
                }else {
                    GameObject.DestroyImmediate(unit.gameObject);
                }                

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
