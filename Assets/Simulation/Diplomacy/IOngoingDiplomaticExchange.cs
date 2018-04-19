using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IOngoingDiplomaticExchange {

        #region events

        event EventHandler<EventArgs> TerminationRequested;

        #endregion

        #region methods

        void Start();
        void End();

        string GetSummary();

        #endregion

    }

}
