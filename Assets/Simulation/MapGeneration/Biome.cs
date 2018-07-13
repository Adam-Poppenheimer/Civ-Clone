using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public struct Biome {

        public CellTerrain    Terrain;
        public CellVegetation Vegetation;

        public Biome(CellTerrain terrain, CellVegetation vegetation) {
            Terrain    = terrain;
            Vegetation = vegetation;
        }

    }

}
