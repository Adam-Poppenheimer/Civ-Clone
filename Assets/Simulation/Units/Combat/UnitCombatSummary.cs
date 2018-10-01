using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class UnitCombatSummary : IUnitCombatSummary {

        #region instance fields and properties

        #region from IUnitCombatSummary

        public bool IgnoresAmphibiousPenalty     { get; set; }
        public bool IgnoresDefensiveTerrainBonus { get; set; }
        public bool IgnoresLineOfSight           { get; set; }

        public bool CanMoveAfterAttacking   { get; set; }
        public bool CanAttackAfterAttacking { get; set; }

        public float GoldRaidingPercentage { get; set; }

        public int BonusRange { get; set; }

        public IEnumerable<ICombatModifier> ModifiersWhenAttacking {
            get { return modifiersWhenAttacking; }
        }
        public List<ICombatModifier> modifiersWhenAttacking = new List<ICombatModifier>();

        public IEnumerable<ICombatModifier> ModifiersWhenDefending {
            get { return modifiersWhenDefending; }
        }
        public List<ICombatModifier> modifiersWhenDefending = new List<ICombatModifier>();

        #endregion

        #endregion

        #region instance methods

        public void Reset() {
            IgnoresAmphibiousPenalty     = false;
            IgnoresDefensiveTerrainBonus = false;

            IgnoresLineOfSight      = false;
            CanMoveAfterAttacking   = false;
            CanAttackAfterAttacking = false;

            GoldRaidingPercentage = 0f;
            BonusRange = 0;

            modifiersWhenAttacking.Clear();
            modifiersWhenDefending.Clear();
        }

        #endregion

    }

}
