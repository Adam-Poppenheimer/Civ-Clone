using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface ICivHomelandTemplate {

        #region properties

        IRegionResourceTemplate StartingResources { get; }
        IRegionResourceTemplate OtherResources    { get; }

        #endregion

    }

}
