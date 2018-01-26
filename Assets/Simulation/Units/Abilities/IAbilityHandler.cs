using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public interface IAbilityHandler {

        #region methods

        bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit);

        AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit);

        #endregion

    }

}
