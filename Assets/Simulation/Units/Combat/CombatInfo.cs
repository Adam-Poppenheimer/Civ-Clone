using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public struct CombatInfo {

        #region instance fields and properties

        public CombatType CombatType;

        public float AttackerCombatModifier;
        public float DefenderCombatModifier;

        public bool AttackerCanMoveAfterAttacking;
        public bool AttackerCanAttackAfterAttacking;
        public bool AttackerIgnoresAmphibiousPenalty;

        public bool DefenderIgnoresDefensiveTerrainBonuses;

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format(
                "\nCombatType: {0}\nAttackerCombatModifier: {1}\nDefenderCombatModifier: {2}\n" + 
                "AttackerCanMOveAfterAttacking: {3}\nAttackerCanAttackAfterAttacking: {4}\n" + 
                "AttackerIgnoresAmphibiousPenalty: {5}\nDefenderIgnoresDefensiveTerrainBonuses: {6}",
                CombatType, AttackerCombatModifier, DefenderCombatModifier, AttackerCanMoveAfterAttacking,
                AttackerCanAttackAfterAttacking, AttackerIgnoresAmphibiousPenalty,
                DefenderIgnoresDefensiveTerrainBonuses
            );
        }

        #endregion

        #endregion

    }

}
