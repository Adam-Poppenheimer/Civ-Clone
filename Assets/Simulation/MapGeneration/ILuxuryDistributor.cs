using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface ILuxuryDistributor {

        #region methods

        void DistributeLuxuriesAcrossHomeland(CivHomelandData homelandData);

        #endregion

    }

}
