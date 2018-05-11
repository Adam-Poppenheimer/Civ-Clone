using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public struct UnitCombatInfo {

        #region instance fields and properties

        public float CombatModifier;

        public bool CanMoveAfterAttacking;
        public bool CanAttackAfterAttacking;
        public bool IgnoresAmphibiousPenalty;

        public bool IgnoresDefensiveTerrainBonuses;

        public float GoldRaidingPercentage;

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format(
                "\nCombatModifier: {0}" + 
                "\nCanMoveAfterAttacking {1}" +
                "\nCanAttackAfterAttacking {2}" +
                "\nIgnoresAmphibiousPenalty {3}" +
                "\nIgnoresDefensiveTerrainBonuses {4}",
                CombatModifier, CanMoveAfterAttacking, CanAttackAfterAttacking,
                IgnoresAmphibiousPenalty, IgnoresDefensiveTerrainBonuses
            );
        }

        #endregion

        #endregion

    }

}
