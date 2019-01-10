using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianUnitSpawner {

        #region methods

        bool TrySpawnUnit(IEncampment encampment);

        #endregion

    }

}
