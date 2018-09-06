using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionResourceTemplate {

        #region properties

        string name { get; }

        float StrategicNodesPerCell  { get; }
        float StrategicCopiesPerCell { get; }

        #endregion

    }

}
