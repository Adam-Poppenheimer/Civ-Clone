using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public interface ICommonCombatExecutionLogic {

        #region methods

        void PerformCommonCombatTasks(IUnit attacker, IUnit defender, CombatInfo combatInfo);

        #endregion

    }

}
