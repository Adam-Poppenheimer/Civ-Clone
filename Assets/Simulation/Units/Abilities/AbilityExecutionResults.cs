using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public struct AbilityExecutionResults {

        #region instance fields and properties

        public bool AbilityHandled { get; private set; }

        public IOngoingAbility NewAbilityActivated { get; private set; }

        #endregion

        #region constructors

        public AbilityExecutionResults(bool abilityHandled, IOngoingAbility newAbilityActivated) {
            AbilityHandled = abilityHandled;
            NewAbilityActivated = newAbilityActivated; 
        }

        #endregion

    }

}
