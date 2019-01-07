using System.Collections.Generic;

using Assets.Simulation.AI;
using Assets.Simulation.Units;

namespace Assets.Simulation.Players.Barbarians {

    public interface IBarbarianUnitBrain {

        #region methods

        List<IUnitCommand> GetCommandsForUnit(IUnit unit, BarbarianInfluenceMaps maps);

        #endregion

    }

}