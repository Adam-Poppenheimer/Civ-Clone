using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IOceanTemplate {

        #region properties

        int DeepOceanLandPercentage { get; }

        IRegionResourceTemplate ArchipelagoResources { get; }

        #endregion

    }

}
