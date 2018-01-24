using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Units.Combat {

    public struct UnitCityCombatData {

        #region instance fields and properties

        public IUnit Attacker { get; private set; }
        public ICity City     { get; private set; }

        public int DamageToAttacker { get; private set; }
        public int DamageToCity     { get; private set; }

        #endregion

        #region constructors

        public UnitCityCombatData(
            IUnit attacker, ICity city,
            int damageToAttacker, int damageToCity
        ){
            Attacker = attacker;
            City = city;
            DamageToAttacker = damageToAttacker;
            DamageToCity = damageToCity;
        }

        #endregion

    }

}
