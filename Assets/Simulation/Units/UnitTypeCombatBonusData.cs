using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units {

    [Serializable]
    public struct UnitTypeCombatBonusData {

        #region instance fields and properties

        public UnitType Type {
            get { return _type; }
        }
        [SerializeField] private UnitType _type;

        public float Bonus {
            get { return _bonus; }
        }
        [SerializeField] private float _bonus;

        #endregion

        #region constructors

        public UnitTypeCombatBonusData(UnitType type, float bonus) {
            _type  = type;
            _bonus = bonus;
        }

        #endregion

    }

}
