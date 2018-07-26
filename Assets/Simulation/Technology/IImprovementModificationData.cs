using Assets.Simulation.Improvements;

namespace Assets.Simulation.Technology {

    public interface IImprovementModificationData {

        #region properties

        YieldSummary      BonusYield { get; }
        IImprovementTemplate Template   { get; }

        bool RequiresFreshWater { get; }

        #endregion

    }

}