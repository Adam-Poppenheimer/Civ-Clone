using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianEncampmentSpawner {

        #region methods

        void TrySpawnEncampment(InfluenceMaps maps);

        #endregion

    }

}
