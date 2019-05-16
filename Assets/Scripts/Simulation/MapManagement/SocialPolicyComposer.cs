using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.MapManagement {

    public class SocialPolicyComposer : ISocialPolicyComposer {

        #region instance fields and properties

        private ISocialPolicyCanon                   PolicyCanon;
        private IEnumerable<IPolicyTreeDefinition>   AvailableTrees;
        private IEnumerable<ISocialPolicyDefinition> AvailablePolicies;

        #endregion

        #region constructors

        [Inject]
        public SocialPolicyComposer(
            ISocialPolicyCanon policyCanon,
            [Inject(Id = "Available Policy Trees")] IEnumerable<IPolicyTreeDefinition>   availableTrees,
            [Inject(Id = "Available Policies"    )] IEnumerable<ISocialPolicyDefinition> availablePolicies
        ){
            PolicyCanon       = policyCanon;
            AvailableTrees    = availableTrees;
            AvailablePolicies = availablePolicies;
        }

        #endregion

        #region instance methods

        #region from ISocialPolicyComposer

        public void ClearPolicyRuntime() {
            PolicyCanon.Clear();
        }

        public SerializableSocialPolicyData ComposePoliciesFromCiv(ICivilization civ) {
            var retval = new SerializableSocialPolicyData() {
                UnlockedTrees    = PolicyCanon.GetTreesUnlockedFor   (civ).Select(tree   => tree  .name).ToList(),
                UnlockedPolicies = PolicyCanon.GetPoliciesUnlockedFor(civ).Select(policy => policy.name).ToList()
            };

            return retval;
        }

        public void DecomposePoliciesIntoCiv(SerializableSocialPolicyData policyData, ICivilization civ) {
            var unlockedTrees    = AvailableTrees   .Where(tree   => policyData.UnlockedTrees   .Contains(tree  .name));
            var unlockedPolicies = AvailablePolicies.Where(policy => policyData.UnlockedPolicies.Contains(policy.name));

            PolicyCanon.OverrideUnlockedTreesForCiv   (unlockedTrees,    civ);
            PolicyCanon.OverrideUnlockedPoliciesForCiv(unlockedPolicies, civ);
        }

        #endregion

        #endregion

    }

}
