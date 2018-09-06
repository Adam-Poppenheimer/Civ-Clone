using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public interface IStrategicDistributor {

        #region methods

        void DistributeStrategicsAcrossHomeland(HomelandData homelandData);

        #endregion

    }

}
