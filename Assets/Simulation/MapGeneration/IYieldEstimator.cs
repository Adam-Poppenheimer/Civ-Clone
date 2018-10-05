using System;
using System.Collections.Generic;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

namespace Assets.Simulation.MapGeneration {

    public interface IYieldEstimator {

        #region methods

        YieldSummary GetYieldEstimateForCell(IHexCell cell, IEnumerable<ITechDefinition> availableTechs);
        YieldSummary GetYieldEstimateForCell(IHexCell cell, CachedTechData techData);

        YieldSummary GetYieldEstimateForResource(IResourceDefinition resource);

        #endregion

    }

}