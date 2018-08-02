using System;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public interface IYieldEstimator {

        #region methods

        YieldSummary GetYieldEstimateForCell(IHexCell cell);

        YieldSummary GetYieldEstimateForResource(IResourceDefinition resource);

        #endregion

    }

}