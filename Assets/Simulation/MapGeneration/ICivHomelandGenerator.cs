using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface ICivHomelandGenerator {

        CivHomelandData GetHomelandData(
            ICivilization civ, List<MapSection> landSections, List<MapSection> waterSections,
            ICivHomelandTemplate template
        );

        void GenerateTopologyAndEcology(CivHomelandData homelandData);

        void DistributeYieldAndResources(CivHomelandData homelandData);

    }

}
