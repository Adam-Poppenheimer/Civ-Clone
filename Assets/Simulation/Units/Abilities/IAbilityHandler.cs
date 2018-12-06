using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public interface IAbilityHandler {

        #region methods

        bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit);
        void HandleCommandOnUnit   (AbilityCommandRequest command, IUnit unit);

        #endregion

    }

}
