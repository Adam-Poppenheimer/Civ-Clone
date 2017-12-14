using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public class ImprovementAbilityHandler : IUnitAbilityHandler {

        #region instance fields and properties



        #endregion

        #region instance methods

        #region from IUnitAbilityHandler

        public bool TryHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            var improvementCommands = ability.CommandRequests.Where(request => request.CommandType == AbilityCommandType.BuildImprovement);
            if(improvementCommands.Count() == 0) {
                return false;
            }else if(improvementCommands.Count() > 1) {
                throw new InvalidOperationException("It's not valid to have two Build Improvement commands on the same ability");
            }else {
                var improvementName = improvementCommands.First().ArgsToPass[0];

                return true;
            }
        }

        #endregion

        #endregion
        
    }

}
