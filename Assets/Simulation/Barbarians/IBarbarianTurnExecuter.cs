using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianTurnExecuter {

        #region methods

        void PerformEncampmentSpawning(InfluenceMaps maps);

        void PerformUnitSpawning();

        #endregion

    }

}
