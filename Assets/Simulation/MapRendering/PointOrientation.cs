using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapRendering {

    public enum PointOrientation {
        Void           = 0,
        Center         = 1,
        Edge           = 2,
        PreviousCorner = 3,
        NextCorner     = 4,
    }

}
