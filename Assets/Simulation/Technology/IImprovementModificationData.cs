using Assets.Simulation.Improvements;

namespace Assets.Simulation.Technology {
    public interface IImprovementModificationData {
        ResourceSummary BonusYield { get; }
        IImprovementTemplate Template { get; }
    }
}