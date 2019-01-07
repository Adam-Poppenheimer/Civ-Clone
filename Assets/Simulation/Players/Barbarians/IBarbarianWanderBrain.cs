using System.Collections.Generic;
using Assets.Simulation.AI;
using Assets.Simulation.Units;

namespace Assets.Simulation.Players.Barbarians {

    public interface IBarbarianWanderBrain {

        #region methods

        List<IUnitCommand> GetWanderCommandsForUnit(IUnit unit, BarbarianInfluenceMaps maps);

        #endregion

    }

}