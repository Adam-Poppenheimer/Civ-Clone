using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianGoalBrain {

        #region methods

        float GetUtilityForUnit(IUnit unit, InfluenceMaps maps);

        List<IUnitCommand> GetCommandsForUnit(IUnit unit, InfluenceMaps maps);

        #endregion

    }

}
