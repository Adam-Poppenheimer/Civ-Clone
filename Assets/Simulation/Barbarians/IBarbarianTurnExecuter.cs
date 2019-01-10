using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianTurnExecuter {

        #region methods

        void PerformEncampmentSpawning(BarbarianInfluenceMaps maps);

        void PerformUnitSpawning();

        #endregion

    }

}
