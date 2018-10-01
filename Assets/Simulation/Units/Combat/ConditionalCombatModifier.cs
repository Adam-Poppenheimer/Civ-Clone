using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.HexMap;
using UnityEngine;

namespace Assets.Simulation.Units.Combat {

    [Serializable]
    public class ConditionalCombatModifier :  ICombatModifier {

        #region internal types

        public enum JoinType {
            Or  = 0,
            And = 1
        }

        #endregion

        #region instance fields and properties

        #region from ICombatModifier

        public float Modifier {
            get { return _modifier; }
        }
        [SerializeField] private float _modifier;

        #endregion

        public List<CombatCondition> Conditions {
            get { return _conditions; }
            set { _conditions = value; }
        }
        [SerializeField] private List<CombatCondition> _conditions;

        public JoinType JoinedTogetherBy {
            get { return _joinedTogetherBy; }
            set { _joinedTogetherBy = value; }
        }
        [SerializeField] private JoinType _joinedTogetherBy;

        #endregion

        #region instance methods

        #region from ICombatModifier

        public bool DoesModifierApply(IUnit subject, IUnit opponent, IHexCell location, CombatType combatType) {
            if(Conditions.Count == 0) {
                return true;
            }

            foreach(var condition in Conditions) {
                if(JoinedTogetherBy == JoinType.And && !condition.IsConditionMet(subject, opponent, location, combatType)) {
                    return false;

                }else if(JoinedTogetherBy == JoinType.Or && condition.IsConditionMet(subject, opponent, location, combatType)) {
                    return true;
                }
            }

            return JoinedTogetherBy == JoinType.And ? true : false;
        }

        #endregion

        #endregion
        
    }

}
