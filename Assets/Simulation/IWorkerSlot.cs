using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation {

    public interface IWorkerSlot {

        #region properties

        bool IsOccupied { get; set; }

        bool IsLocked { get; set; }

        ResourceSummary BaseYield { get; }

        #endregion

    }

}
