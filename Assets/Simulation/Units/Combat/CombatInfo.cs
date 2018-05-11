using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public class CombatInfo {

        #region instance fields and properties

        public CombatType CombatType;

        public UnitCombatInfo Attacker = new UnitCombatInfo();

        public UnitCombatInfo Defender = new UnitCombatInfo();

        #endregion

        #region instance methods

        #region from Object

        public override bool Equals(object obj) {
            var otherInfo = obj as CombatInfo;

            return otherInfo != null && otherInfo.CombatType == CombatType 
                && otherInfo.Attacker.Equals(Attacker) && otherInfo.Defender.Equals(Defender);
        }

        public override int GetHashCode() {
            int hash = 13;

            hash = (hash * 7) + Attacker.GetHashCode();
            hash = (hash * 7) + Defender.GetHashCode();

            return hash;
        }

        #endregion

        #endregion

    }

}
