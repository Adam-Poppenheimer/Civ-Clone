﻿using System;
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
            if(ability.CommandRequests.Any(request => request.CommandType == AbilityCommandType.SetUpToBombard)) {
                return unit.CurrentMovement > 0 && !unit.IsSetUpToBombard;
            }else {
                return false;
            }
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                unit.SetUpToBombard();
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
