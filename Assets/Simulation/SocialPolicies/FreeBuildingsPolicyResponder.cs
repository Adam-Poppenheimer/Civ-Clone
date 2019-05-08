using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.SocialPolicies {

    public class FreeBuildingsPolicyResponder {

        #region instance fields and properties

        private IFreeBuildingsCanon FreeBuildingsCanon;

        #endregion

        #region constructors

        [Inject]
        public FreeBuildingsPolicyResponder(
            IFreeBuildingsCanon freeBuildingsCanon, CivilizationSignals civSignals
        ) {
            FreeBuildingsCanon = freeBuildingsCanon;

            civSignals.CivUnlockedPolicy.Subscribe(OnCivUnlockedPolicy);
            civSignals.CivLockedPolicy  .Subscribe(OnCivLockedPolicy);

            civSignals.CivUnlockedPolicyTree.Subscribe(OnCivUnlockedPolicyTree);
            civSignals.CivLockedPolicyTree  .Subscribe(OnCivLockedPolicyTree);

            civSignals.CivFinishedPolicyTree  .Subscribe(OnCivFinishedPolicyTree);
            civSignals.CivUnfinishedPolicyTree.Subscribe(OnCivUnfinishedPolicyTree);
        }

        #endregion

        #region instance methods

        private void OnCivUnlockedPolicy(UniRx.Tuple<ICivilization, ISocialPolicyDefinition> data) {
            var civ    = data.Item1;
            var policy = data.Item2;

            ApplyFreeBuildingsFromBonuses(policy.Bonuses, civ);
        }

        private void OnCivLockedPolicy(UniRx.Tuple<ICivilization, ISocialPolicyDefinition> data) {
            var civ    = data.Item1;
            var policy = data.Item2;

            RevokeFreeBuildingsFromBonuses(policy.Bonuses, civ);
        }

        private void OnCivUnlockedPolicyTree(UniRx.Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ        = data.Item1;
            var policyTree = data.Item2;

            ApplyFreeBuildingsFromBonuses(policyTree.UnlockingBonuses, civ);
        }

        private void OnCivLockedPolicyTree(UniRx.Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ        = data.Item1;
            var policyTree = data.Item2;

            RevokeFreeBuildingsFromBonuses(policyTree.UnlockingBonuses, civ);
        }

        private void OnCivFinishedPolicyTree(UniRx.Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ        = data.Item1;
            var policyTree = data.Item2;

            ApplyFreeBuildingsFromBonuses(policyTree.CompletionBonuses, civ);
        }

        private void OnCivUnfinishedPolicyTree(UniRx.Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ        = data.Item1;
            var policyTree = data.Item2;

            RevokeFreeBuildingsFromBonuses(policyTree.CompletionBonuses, civ);
        }

        private void ApplyFreeBuildingsFromBonuses(ISocialPolicyBonusesData bonuses, ICivilization civ) {
            for(int i = 0; i < bonuses.FreeBuildingCount; i++) {
                FreeBuildingsCanon.SubscribeFreeBuildingToCiv(bonuses.FreeBuildingTemplates, civ);
            }
        }

        private void RevokeFreeBuildingsFromBonuses(ISocialPolicyBonusesData bonuses, ICivilization civ) {
            for(int i = 0; i < bonuses.FreeBuildingCount; i++) {
                FreeBuildingsCanon.RemoveFreeBuildingFromCiv(bonuses.FreeBuildingTemplates, civ);
            }
        }

        #endregion

    }

}
