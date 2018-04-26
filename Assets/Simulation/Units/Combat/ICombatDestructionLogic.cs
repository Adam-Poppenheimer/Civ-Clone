using System;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatDestructionLogic {

        #region methods

        void HandleUnitDestructionFromCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo);

        #endregion

    }

}