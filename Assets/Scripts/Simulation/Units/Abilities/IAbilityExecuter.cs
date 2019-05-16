using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public interface IAbilityExecuter {

        #region methods

        bool CanExecuteAbilityOnUnit(IAbilityDefinition ability, IUnit unit);

        void ExecuteAbilityOnUnit(IAbilityDefinition ability, IUnit unit);

        #endregion

    }

}
