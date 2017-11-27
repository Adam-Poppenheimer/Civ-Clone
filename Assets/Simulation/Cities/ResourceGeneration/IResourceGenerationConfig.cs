using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.ResourceGeneration {

    public interface IResourceGenerationConfig {

        #region properties

        ResourceSummary UnemployedYield { get; }

        #endregion

    }

}
