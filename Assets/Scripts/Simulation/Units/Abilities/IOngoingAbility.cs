using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public interface IOngoingAbility {

        #region methods

        bool IsReadyToTerminate();

        void BeginExecution();
        void TickExecution();
        void TerminateExecution();

        #endregion

    }

}
