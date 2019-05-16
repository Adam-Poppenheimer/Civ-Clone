using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public class CombatInfo {

        #region instance fields and properties

        public CombatType CombatType;

        public float AttackerCombatModifier;
        public float DefenderCombatModifier;

        #endregion

        #region instance methods

        #region from Object

        public override bool Equals(object obj) {
            var otherInfo = obj as CombatInfo;

            return otherInfo != null
                && otherInfo.CombatType == CombatType 
                && otherInfo.AttackerCombatModifier.Equals(AttackerCombatModifier)
                && otherInfo.DefenderCombatModifier.Equals(DefenderCombatModifier);
        }

        public override int GetHashCode() {
            int hash = 13;

            hash = (hash * 7) + AttackerCombatModifier.GetHashCode();
            hash = (hash * 7) + DefenderCombatModifier.GetHashCode();

            return hash;
        }

        public override string ToString() {
            return string.Format(
                "CombatType: {0} | AttackerCombatModifier: {1} | DefenderCombatModifier: {2}",
                CombatType, AttackerCombatModifier, DefenderCombatModifier
            );
        }

        #endregion

        #endregion

    }

}
