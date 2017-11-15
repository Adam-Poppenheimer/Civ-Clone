using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface IBuilding {

        #region properties

        ResourceSummary StaticYield { get; }

        ReadOnlyCollection<IWorkerSlot> Slots { get; }

        #endregion

    }

}
