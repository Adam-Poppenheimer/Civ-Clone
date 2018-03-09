using Assets.Simulation;

namespace Assets.UI {

    public interface IYieldFormatter {

        #region methods

        string GetTMProFormattedYieldString(ResourceSummary summary, bool includeEmptyValues = false);

        #endregion

    }

}