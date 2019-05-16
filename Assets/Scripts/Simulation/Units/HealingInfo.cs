using System;

namespace Assets.Simulation.Units {

    public class HealingInfo {

        #region instance fields and properties

        public bool HealsEveryTurn;

        public int BonusHealingToSelf;

        public int BonusHealingToAdjacent;

        public int AlternateNavalBaseHealing;

        #endregion

        #region instance methods

        #region from Object

        public override bool Equals(object obj) {
            var otherAsInfo = obj as HealingInfo;

            return otherAsInfo != null
                && HealsEveryTurn            == otherAsInfo.HealsEveryTurn
                && BonusHealingToSelf        == otherAsInfo.BonusHealingToSelf
                && BonusHealingToAdjacent    == otherAsInfo.BonusHealingToAdjacent
                && AlternateNavalBaseHealing == otherAsInfo.AlternateNavalBaseHealing;
        }

        public override int GetHashCode() {
            int hash = 13;

            hash = (hash * 7) + HealsEveryTurn           .GetHashCode();
            hash = (hash * 7) + BonusHealingToSelf       .GetHashCode();
            hash = (hash * 7) + BonusHealingToAdjacent   .GetHashCode();
            hash = (hash * 7) + AlternateNavalBaseHealing.GetHashCode();

            return hash;
        }

        #endregion

        #endregion

    }

}