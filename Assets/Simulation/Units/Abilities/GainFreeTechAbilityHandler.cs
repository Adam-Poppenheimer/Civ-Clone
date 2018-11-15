using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Units.Abilities {

    public class GainFreeTechAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ITechCanon                                    TechCanon;

        #endregion

        #region constructors

        [Inject]
        public GainFreeTechAbilityHandler(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon, ITechCanon techCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            TechCanon           = techCanon;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            return ability.CommandRequests.Any(request => request.CommandType == AbilityCommandType.GainFreeTech);
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                TechCanon.AddFreeTechToCiv(unitOwner);

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
