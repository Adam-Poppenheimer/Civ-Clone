using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    [Serializable]
    public class PermanentCombatModifier : ICombatModifier {

        #region instance fields and properties

        #region from ICombatModifier

        public float Modifier {
            get { return _modifier; }
        }
        [SerializeField] private float _modifier = 0f;

        #endregion

        #endregion

        #region instance methods

        #region from ICombatModifier

        public bool DoesModifierApply(
            IUnit candidate, IUnit opponent, IHexCell location, CombatType combatType
        ) {
            return true;
        }

        #endregion

        #endregion

        
    }

}
