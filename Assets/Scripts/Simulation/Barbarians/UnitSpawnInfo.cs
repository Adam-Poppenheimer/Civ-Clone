using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public struct UnitSpawnInfo {

        public bool IsValidSpawn;

        public IHexCell      LocationOfUnit;
        public IUnitTemplate TemplateToBuild;

    }

}
