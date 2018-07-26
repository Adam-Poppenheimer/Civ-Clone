using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.Cities.Buildings {

    public interface IResourceYieldModificationData {

        #region instance fields and properties

        IResourceDefinition Resource   { get; }
        YieldSummary              BonusYield { get; }

        #endregion

    }

}
