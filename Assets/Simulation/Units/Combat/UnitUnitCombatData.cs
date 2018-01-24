using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public struct UnitUnitCombatData {

        #region instance fields and properties

        public IUnit Attacker { get; private set; }
        public IUnit Defender { get; private set; }

        public int DamageToAttacker { get; private set; }
        public int DamageToDefender { get; private set; }

        #endregion

        #region constructors

        public UnitUnitCombatData(IUnit attacker, IUnit defender,
            int damageToAttacker, int damageToDefender
        ){
            Attacker = attacker;
            Defender = defender;
            DamageToAttacker = damageToAttacker;
            DamageToDefender = damageToDefender;
        }

        #endregion

    }

}
