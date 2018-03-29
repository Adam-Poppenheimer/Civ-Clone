using Assets.Simulation.Improvements;

namespace Assets.Simulation.Technology {

    public interface IImprovementModificationData {

        #region properties

        ResourceSummary      BonusYield { get; }
        IImprovementTemplate Template   { get; }

        bool RequiresFreshWater { get; }

        #endregion

    }

}