using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerationTemplate {

        #region properties

        int CivCount { get; }

        IEnumerable<MapSection> ContinentSections { get; }

        IEnumerable<IContinentGenerationTemplate> ContinentTemplates { get; }
        IEnumerable<IOceanGenerationTemplate>     OceanTemplates     { get; }

        #endregion

    }

}
