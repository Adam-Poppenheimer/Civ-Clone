using System.Collections.Generic;

using Assets.Simulation;

namespace Assets.UI {

    public interface IYieldFormatter {

        #region methods

        string GetTMProFormattedYieldString(
            YieldSummary summary, IEnumerable<YieldType> typesToDisplay,
            bool includeEmptyValues = false, bool plusOnPositiveNumbers = false
        );

        string GetTMProFormattedSingleResourceString(YieldType type, float value);

        string GetTMProFormattedHappinessString(int netHappiness);

        #endregion

    }

}