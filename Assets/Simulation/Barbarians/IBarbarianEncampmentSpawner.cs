using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianEncampmentSpawner {

        #region methods

        void TrySpawnEncampment(BarbarianInfluenceMaps maps);

        #endregion

    }

}
