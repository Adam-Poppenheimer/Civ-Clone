using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public struct UnitCombatResults {

        #region instance fields and properties

        public IUnit Attacker { get; private set; }
        public IUnit Defender { get; private set; }

        public int DamageToAttacker { get; private set; }
        public int DamageToDefender { get; private set; }

        public CombatInfo InfoOfAttack { get; private set; }

        #endregion

        #region constructors

        public UnitCombatResults(IUnit attacker, IUnit defender,
            int damageToAttacker, int damageToDefender,
            CombatInfo infoOfAttack
        ){
            Attacker = attacker;
            Defender = defender;
            DamageToAttacker = damageToAttacker;
            DamageToDefender = damageToDefender;
            InfoOfAttack = infoOfAttack;
        }

        #endregion

    }

}
