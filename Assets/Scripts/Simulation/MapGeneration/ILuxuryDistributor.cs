using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface ILuxuryDistributor {

        #region methods

        void DistributeLuxuriesAcrossHomeland(HomelandData homelandData);

        #endregion

    }

}
