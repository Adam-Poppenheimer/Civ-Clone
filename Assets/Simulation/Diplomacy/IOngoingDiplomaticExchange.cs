using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Diplomacy {

    public interface IOngoingDiplomaticExchange {

        #region properties

        ExchangeType Type { get; }

        ICivilization Sender   { get; set; }
        ICivilization Receiver { get; set; }

        int IntegerInput { get; set; }

        IResourceDefinition ResourceInput { get; set; }

        #endregion

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
