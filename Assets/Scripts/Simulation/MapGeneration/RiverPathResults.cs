using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public struct RiverPathResults {

        #region static fields and properties

        public static readonly RiverPathResults Success = new RiverPathResults(true,  false);
        public static readonly RiverPathResults Fail    = new RiverPathResults(false, false);
        public static readonly RiverPathResults Water   = new RiverPathResults(true,  true);

        #endregion

        #region instance fields and properties

        public bool Completed;
        public bool FoundWater;

        #endregion

        #region constructors

        public RiverPathResults(bool completed, bool foundWater) {
            Completed  = completed;
            FoundWater = foundWater;
        }

        #endregion

    }
}
