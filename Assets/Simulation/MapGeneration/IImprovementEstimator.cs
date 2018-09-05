using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public interface IImprovementEstimator {

        #region methods

        IImprovementTemplate GetExpectedImprovementForCell(IHexCell cell, IResourceNode nodeAtLocation);

        #endregion

    }
}