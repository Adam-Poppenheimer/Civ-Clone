using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public interface IUnitAbilityExecuter {

        #region methods

        void ExecuteAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit);

        #endregion

    }

}
