using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public interface IPostCombatMovementLogic {

        #region methods

        void HandleAttackerMovementAfterCombat(
            IUnit attacker, IUnit defender, CombatInfo combatInfo
        );

        #endregion

    }

}