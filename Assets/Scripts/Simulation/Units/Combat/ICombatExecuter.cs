using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatExecuter {

        #region methods

        bool CanPerformMeleeAttack(IUnit attacker, IUnit defender);
        void PerformMeleeAttack   (IUnit attacker, IUnit defender, Action successAction, Action failAction);

        bool CanPerformRangedAttack(IUnit attacker, IUnit defender);
        void PerformRangedAttack   (IUnit attacker, IUnit defender);

        #endregion

    }

}
