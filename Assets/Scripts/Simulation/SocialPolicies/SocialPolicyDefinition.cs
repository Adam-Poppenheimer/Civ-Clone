using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.SocialPolicies {

    [CreateAssetMenu(menuName = "Civ Clone/Social Policies/Policy")]
    public class SocialPolicyDefinition : ScriptableObject, ISocialPolicyDefinition {

        #region instance fields and properties

        #region from ISocialPolicyDefinition

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] string _description = null;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon = null;

        public IEnumerable<ISocialPolicyDefinition> Prerequisites {
            get { return _prerequisites.Cast<ISocialPolicyDefinition>(); }
        }
        [SerializeField] private List<SocialPolicyDefinition> _prerequisites = null;

        public float TreeNormalizedX {
            get { return _treeNormalizedX; }
        }
        [SerializeField] private float _treeNormalizedX = 0f;

        public float TreeNormalizedY {
            get { return _treeNormalizedY; }
        }
        [SerializeField] private float _treeNormalizedY = 0f;

        public ISocialPolicyBonusesData Bonuses {
            get { return _bonuses; }
        }
        [SerializeField] private SocialPolicyBonusesData _bonuses = new SocialPolicyBonusesData();

        #endregion

        #endregion
        
    }

}
