using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IOceanGenerator {

        #region methods

        void GenerateOcean(MapSection ocean, IOceanGenerationTemplate template);

        #endregion

    }

}
