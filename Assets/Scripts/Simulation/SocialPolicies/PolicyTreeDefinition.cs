using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.SocialPolicies {

    [CreateAssetMenu(menuName = "Civ Clone/Social Policies/Policy Tree")]
    public class PolicyTreeDefinition : ScriptableObject, IPolicyTreeDefinition {

        #region instance fields and properties

        #region from IPolicyTreeDefinition

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        public IEnumerable<ISocialPolicyDefinition> Policies {
            get { return _policies.Cast<ISocialPolicyDefinition>(); }
        }
        [SerializeField] private List<SocialPolicyDefinition> _policies;

        public int Row {
            get { return _row; }
        }
        [SerializeField] private int _row;

        public ISocialPolicyBonusesData UnlockingBonuses {
            get { return _unlockingBonuses; }
        }
        [SerializeField] private SocialPolicyBonusesData _unlockingBonuses;

        public ISocialPolicyBonusesData CompletionBonuses {
            get { return _completionBonuses; }
        }
        [SerializeField] private SocialPolicyBonusesData _completionBonuses;      

        #endregion

        #endregion
        
    }

}
