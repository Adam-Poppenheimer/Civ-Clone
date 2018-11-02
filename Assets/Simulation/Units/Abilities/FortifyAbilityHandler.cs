using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public class FortifyAbilityHandler : IAbilityHandler {

        #region constructors



        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(ability.CommandRequests.Any(request => request.CommandType == AbilityCommandType.Fortify)) {
                return unit.CurrentMovement > 0 && !unit.IsFortified;
            }else {
                return false;
            }
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                unit.BeginFortifying();

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
