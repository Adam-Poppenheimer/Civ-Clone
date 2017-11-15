using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface IWorkerSlot {

        #region properties

        bool IsOccupied { get; set; }

        ResourceSummary BaseYield { get; }

        #endregion

    }

}
