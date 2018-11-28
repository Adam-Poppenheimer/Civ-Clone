using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Cities.Production {

    [Serializable]
    public class ProductionModifier : IProductionModifier {

        #region internal types

        public enum JoinType {
            Or  = 0,
            And = 1,
        }

        #endregion

        #region instance fields and properties

        #region from IProductionModifier

        public float Value {
            get { return _value; }
        }
        [SerializeField] private float _value;

        #endregion

        public List<ProductionCondition> Conditions {
            get { return _conditions; }
            set { _conditions = value; }
        }
        [SerializeField] private List<ProductionCondition> _conditions;

        public JoinType JoinedTogetherBy {
            get { return _joinedTogetherBy; }
            set { _joinedTogetherBy = value; }
        }
        [SerializeField] private JoinType _joinedTogetherBy;

        #endregion

        #region instance methods

        #region from IProductionModifier

        public bool DoesModifierApply(IProductionProject project, ICity city) {
            if(Conditions == null || Conditions.Count == 0) {
                return true;
            }

            foreach(var condition in Conditions) {
                if(JoinedTogetherBy == JoinType.And && !condition.IsMetBy(project, city)) {
                    return false;

                }else if(JoinedTogetherBy == JoinType.Or && condition.IsMetBy(project, city)) {
                    return true;
                }
            }
            
            return JoinedTogetherBy == JoinType.And ? true : false;
        }

        #endregion

        #endregion

    }

}
