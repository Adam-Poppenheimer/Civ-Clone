using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Simulation.SocialPolicies {

    public interface ISocialPolicyBonusLogic {

        #region methods

        YieldSummary GetBonusCityYieldForCiv   (ICivilization civ);
        YieldSummary GetBonusCapitalYieldForCiv(ICivilization civ);

        T ExtractBonusFromCiv<T>(
            ICivilization civ, Func<ISocialPolicyBonusesData, T> extractor, Func<T, T, T> aggregator
        );

        #endregion

    }

}
