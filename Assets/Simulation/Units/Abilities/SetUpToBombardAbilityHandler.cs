using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public class SetUpToBombardAbilityHandler : IAbilityHandler {

        #region instance fields and properties



        #endregion

        #region constructors

        public SetUpToBombardAbilityHandler() { }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(ability.CommandRequests.Where(request => request.CommandType == AbilityCommandType.SetUpToBombard).Count() != 0) {
                return unit.CurrentMovement > 0;
            }else {
                return false;
            }
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                unit.Animator.SetTrigger("Set Up To Fire Requested");
                unit.CurrentMovement -= 1;             

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        #endregion
        
    }

}
