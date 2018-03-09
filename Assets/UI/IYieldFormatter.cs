using Assets.Simulation;

namespace Assets.UI {

    public interface IYieldFormatter {

        #region methods

        string GetTMProFormattedYieldString(ResourceSummary summary, bool includeEmptyValues = false, bool plusOnPositiveNumbers = false);

        string GetTMProFormattedSingleResourceString(ResourceType type, float value);

        #endregion

    }

}