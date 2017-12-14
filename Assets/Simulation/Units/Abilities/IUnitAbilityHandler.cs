using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public interface IUnitAbilityHandler {

        #region methods

        bool TryHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit);

        #endregion

    }

}
