using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public interface IPostCombatResponder {

        #region methods

        void RespondToCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo);

        #endregion

    }

}
