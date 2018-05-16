using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public class UnitCombatInfo {

        #region instance fields and properties

        public float CombatModifier;

        public bool CanMoveAfterAttacking;
        public bool CanAttackAfterAttacking;
        public bool IgnoresAmphibiousPenalty;

        public bool IgnoresDefensiveTerrainBonuses;

        public float GoldRaidingPercentage;

        public bool IgnoresLineOfSight;

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

        public override bool Equals(object obj) {
            var otherInfo = obj as UnitCombatInfo;

            return otherInfo != null
                && CombatModifier                 == otherInfo.CombatModifier
                && CanMoveAfterAttacking          == otherInfo.CanMoveAfterAttacking
                && CanAttackAfterAttacking        == otherInfo.CanAttackAfterAttacking
                && IgnoresAmphibiousPenalty       == otherInfo.IgnoresAmphibiousPenalty
                && IgnoresDefensiveTerrainBonuses == otherInfo.IgnoresDefensiveTerrainBonuses
                && GoldRaidingPercentage          == otherInfo.GoldRaidingPercentage
                && IgnoresLineOfSight             == otherInfo.IgnoresLineOfSight;
        }

        public override int GetHashCode() {
            int hash = 13;

            hash = (hash * 7) + CombatModifier                .GetHashCode();
            hash = (hash * 7) + CanMoveAfterAttacking         .GetHashCode();
            hash = (hash * 7) + CanAttackAfterAttacking       .GetHashCode();
            hash = (hash * 7) + IgnoresAmphibiousPenalty      .GetHashCode();
            hash = (hash * 7) + IgnoresDefensiveTerrainBonuses.GetHashCode();
            hash = (hash * 7) + GoldRaidingPercentage         .GetHashCode();
            hash = (hash * 7) + IgnoresLineOfSight            .GetHashCode();

            return hash;
        }

        #endregion

        #endregion

    }

}
