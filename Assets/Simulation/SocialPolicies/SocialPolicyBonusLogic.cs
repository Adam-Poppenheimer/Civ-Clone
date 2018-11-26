using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.SocialPolicies {

    public class SocialPolicyBonusLogic : ISocialPolicyBonusLogic {

        #region instance fields and properties

        private ISocialPolicyCanon SocialPolicyCanon;

        #endregion

        #region constructors

        [Inject]
        public SocialPolicyBonusLogic(ISocialPolicyCanon socialPolicyCanon) {
            SocialPolicyCanon = socialPolicyCanon;
        }

        #endregion

        #region static methods

        private static YieldSummary SummaryAdd(YieldSummary a, YieldSummary b) { return a + b; }
        private static float        FloatAdd  (float        a, float        b) { return a + b; }

        #endregion

        #region instance methods

        #region from ISocialPolicyYieldLogic

        public YieldSummary GetBonusCityYieldForCiv(ICivilization civ) {
            return ExtractBonusFromCiv(civ, data => data.CityYield, SummaryAdd);
        }

        public YieldSummary GetBonusCapitalYieldForCiv(ICivilization civ) {
            return ExtractBonusFromCiv(civ, data => data.CapitalYield, SummaryAdd);
        }

        public T ExtractBonusFromCiv<T>(
            ICivilization civ, Func<ISocialPolicyBonusesData, T> extractor, Func<T, T, T> aggregator
        ) {
            T retval = default(T);

            foreach(var policy in SocialPolicyCanon.GetPoliciesUnlockedFor(civ)) {
                retval = aggregator(retval, extractor(policy.Bonuses));
            }

            foreach(var policyTree in SocialPolicyCanon.GetTreesUnlockedFor(civ)) {
                retval = aggregator(retval, extractor(policyTree.UnlockingBonuses));

                if(SocialPolicyCanon.IsTreeCompletedByCiv(policyTree, civ)) {
                    retval = aggregator(retval, extractor(policyTree.CompletionBonuses));
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
