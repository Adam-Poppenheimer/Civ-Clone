using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapRendering {

    [Flags]
    public enum TerrainRefreshType {
        None       = 0,
        Alphamap   = 1,
        Heightmap  = 2,
        Water      = 4,
        Culture    = 8,
        Features   = 16,
        Visibility = 32,
        Farmland   = 64,
        Roads      = 128,
        Rivers     = 256,
        Marshes    = 512,
        Oases      = 1024,

        All = Alphamap | Heightmap | Water | Culture | Features | Visibility | Farmland | Roads | Rivers | Marshes | Oases
    }

}
