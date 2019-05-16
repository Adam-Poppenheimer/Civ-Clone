using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.AI {

    public interface IUnitCommand {

        #region properties

        CommandStatus Status { get; }

        #endregion

        #region methods

        void StartExecution();

        #endregion

    }

}
