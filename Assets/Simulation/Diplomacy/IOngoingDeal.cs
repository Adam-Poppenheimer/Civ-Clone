using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IOngoingDeal {

        #region properties

        ICivilization Sender   { get; }
        ICivilization Receiver { get; }

        IEnumerable<IOngoingDiplomaticExchange> ExchangesFromSender { get; }
        IEnumerable<IOngoingDiplomaticExchange> ExchangesFromReceiver { get; }
        IEnumerable<IOngoingDiplomaticExchange> BilateralExchanges  { get; }

        int TurnsLeft { get; set; }

        #endregion

        #region events

        event EventHandler<OngoingDealEventArgs> TerminationRequested;

        #endregion

        #region methods

        void Start();
        void End();

        #endregion

    }

}
