using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Cities;

namespace Assets.Cities.Buildings {

    public interface IBuildingTemplate {

        #region properties

        int Cost { get; }
        int Maintenance { get; }

        ResourceSummary StaticYield { get; }

        ReadOnlyCollection<ResourceSummary> SlotYields { get; }

        #endregion

    }

}
