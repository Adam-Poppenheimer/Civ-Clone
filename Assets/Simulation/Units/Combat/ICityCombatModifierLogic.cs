using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public interface ICityCombatModifierLogic {

        #region methods

        void ApplyCityModifiersToCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo);

        #endregion

    }

}
