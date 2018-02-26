using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Cities.Buildings {

    public interface IResourceYieldModificationData {

        #region instance fields and properties

        ISpecialtyResourceDefinition Resource   { get; }
        ResourceSummary              BonusYield { get; }

        #endregion

    }

}
