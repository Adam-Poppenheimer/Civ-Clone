using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IOceanGenerator {

        #region methods

        void GenerateOcean(MapRegion ocean);

        #endregion

    }

}
