using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public interface ICivHomelandGenerator {

        CivHomelandData GetHomelandData(
            ICivilization civ, List<MapSection> landSections, List<MapSection> waterSections,
            GridPartition partition, ICivHomelandTemplate homelandTemplate, IMapTemplate mapTemplate
        );

        void GenerateTopologyAndEcology(CivHomelandData homelandData, IMapTemplate mapTemplate);

        void DistributeYieldAndResources(CivHomelandData homelandData, IMapTemplate mapTemplate);

    }

}
