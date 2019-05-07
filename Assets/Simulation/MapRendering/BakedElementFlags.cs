using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapRendering {

    [Flags]
    public enum BakedElementFlags {
        None        = 0,
        Culture     = 1,
        Farmland    = 2,
        Roads       = 4,
        Riverbanks  = 8,
        SimpleLand  = 16,
        SimpleWater = 32
    }

}
