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

            civSignals.CivUnlockedPolicySignal.Subscribe(OnCivUnlockedPolicy);
            civSignals.CivLockedPolicySignal  .Subscribe(OnCivLockedPolicy);

            civSignals.CivUnlockedPolicyTreeSignal.Subscribe(OnCivUnlockedPolicyTree);
            civSignals.CivLockedPolicyTreeSignal  .Subscribe(OnCivLockedPolicyTree);

            civSignals.CivFinishedPolicyTreeSignal  .Subscribe(OnCivFinishedPolicyTree);
            civSignals.CivUnfinishedPolicyTreeSignal.Subscribe(OnCivUnfinishedPolicyTree);
        }

        #endregion

        #region instance methods

        private void OnCivUnlockedPolicy(Tuple<ICivilization, ISocialPolicyDefinition> data) {
            var civ    = data.Item1;
            var policy = data.Item2;

            ApplyFreeBuildingsFromBonuses(policy.Bonuses, civ);
        }

        private void OnCivLockedPolicy(Tuple<ICivilization, ISocialPolicyDefinition> data) {
            var civ    = data.Item1;
            var policy = data.Item2;

            RevokeFreeBuildingsFromBonuses(policy.Bonuses, civ);
        }

        private void OnCivUnlockedPolicyTree(Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ        = data.Item1;
            var policyTree = data.Item2;

            ApplyFreeBuildingsFromBonuses(policyTree.UnlockingBonuses, civ);
        }

        private void OnCivLockedPolicyTree(Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ        = data.Item1;
            var policyTree = data.Item2;

            RevokeFreeBuildingsFromBonuses(policyTree.UnlockingBonuses, civ);
        }

        private void OnCivFinishedPolicyTree(Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ        = data.Item1;
            var policyTree = data.Item2;

            ApplyFreeBuildingsFromBonuses(policyTree.CompletionBonuses, civ);
        }

        private void OnCivUnfinishedPolicyTree(Tuple<ICivilization, IPolicyTreeDefinition> data) {
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
