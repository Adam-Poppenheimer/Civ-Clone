using System.Collections.Generic;

namespace Assets.Simulation.MapGeneration {

    public interface IContinentGenerationTemplate {

        #region properties

        int StartingAreaCount { get; }

        IEnumerable<IRegionGenerationTemplate> StartingLocationTemplates { get; }
        IEnumerable<IRegionGenerationTemplate> BoundaryTemplates         { get; }

        #endregion

    }

}