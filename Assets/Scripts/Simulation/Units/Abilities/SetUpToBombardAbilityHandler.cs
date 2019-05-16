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

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(command.Type == AbilityCommandType.SetUpToBombard) {
                return unit.CurrentMovement > 0 && !unit.IsSetUpToBombard;
            }else {
                return false;
            }
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                unit.SetUpToBombard();
                unit.CurrentMovement -= 1;
            }else {
                throw new InvalidOperationException("Cannot handle command");
            }
        }

        #endregion

        #endregion
        
    }

}
