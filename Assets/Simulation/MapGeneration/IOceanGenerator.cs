using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IOceanGenerator {

        #region methods

        OceanData GetOceanData(
            IEnumerable<MapSection> oceanSections, IOceanTemplate oceanTemplate,
            IMapTemplate mapTemplate, GridPartition partition
        );

        void GenerateTopologyAndEcology(OceanData data);

        void DistributeYieldAndResources(OceanData data);

        #endregion

    }

}
