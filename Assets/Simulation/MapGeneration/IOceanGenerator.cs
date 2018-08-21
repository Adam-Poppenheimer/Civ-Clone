using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IOceanGenerator {

        #region methods

        OceanData GetOceanData(IEnumerable<MapSection> oceanSections, IMapTemplate mapTemplate);

        void GenerateTopologyAndEcology(OceanData data);

        void DistributeYieldAndResources(OceanData data);

        #endregion

    }

}
