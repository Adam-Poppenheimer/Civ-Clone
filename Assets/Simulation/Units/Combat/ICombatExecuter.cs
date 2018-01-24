using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatExecuter {

        #region methods

        bool CanPerformMeleeAttack(IUnit attacker, IUnit defender);
        bool CanPerformMeleeAttack(IUnit attacker, ICity city);

        void PerformMeleeAttack   (IUnit attacker, IUnit defender);
        void PerformMeleeAttack   (IUnit attacker, ICity city);

        bool CanPerformRangedAttack(IUnit attacker, IUnit defender);
        bool CanPerformRangedAttack(IUnit attacker, ICity city);

        void PerformRangedAttack   (IUnit attacker, IUnit defender);
        void PerformRangedAttack   (IUnit attacker, ICity city);

        #endregion

    }

}
