using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.AI {

    public interface IUnitCommandExecuter {

        #region methods

        List<IUnitCommand> GetCommandsForUnit(IUnit unit);

        void SetCommandsForUnit(IUnit unit, List<IUnitCommand> commands);

        void ClearCommandsForUnit(IUnit unit);

        void IterateAllCommands(Action postExecutionAction);

        #endregion

    }

}
