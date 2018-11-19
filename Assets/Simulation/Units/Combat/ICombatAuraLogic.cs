using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatAuraLogic {

        #region methods

        void ApplyAurasToCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo);

        #endregion

    }

}