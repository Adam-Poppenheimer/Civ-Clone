using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public interface IHomelandGenerator {

        HomelandData GetHomelandData(
            ICivilization civ, List<MapSection> landSections, List<MapSection> waterSections,
            GridPartition partition, IHomelandTemplate homelandTemplate, IMapTemplate mapTemplate
        );

        void GenerateTopologyAndEcology(HomelandData homelandData, IMapTemplate mapTemplate);

        void DistributeYieldAndResources(HomelandData homelandData, IMapTemplate mapTemplate);

    }

}
