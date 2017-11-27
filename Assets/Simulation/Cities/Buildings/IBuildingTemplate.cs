using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Buildings {

    public interface IBuildingTemplate {

        #region properties

        string name { get; }

        int Cost { get; }
        int Maintenance { get; }

        ResourceSummary StaticYield { get; }

        ReadOnlyCollection<ResourceSummary> SlotYields { get; }

        #endregion

    }

}
