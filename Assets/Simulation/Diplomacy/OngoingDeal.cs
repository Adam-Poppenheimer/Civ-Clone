using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class OngoingDeal : IOngoingDeal {

        #region instance fields and properties

        #region from IOngoingDiplomaticExchange

        public ICivilization Sender { get; private set; }

        public ICivilization Receiver { get; private set; }

        public IEnumerable<IOngoingDiplomaticExchange> ExchangesFromSender   { get; set; }
        public IEnumerable<IOngoingDiplomaticExchange> ExchangesFromReceiver { get; set; }
        public IEnumerable<IOngoingDiplomaticExchange> BilateralExchanges    { get; set; }

        public int TurnsLeft { get; set; }

        #endregion

        #endregion

        #region events

        #region from IOngoingDiplomaticExchange

        public event EventHandler<OngoingDealEventArgs> TerminationRequested;

        #endregion

        #endregion

        #region constructors

        public OngoingDeal(
            ICivilization sender, ICivilization receiver, List<IOngoingDiplomaticExchange> fromSender,
            List<IOngoingDiplomaticExchange> fromReceiver, List<IOngoingDiplomaticExchange> bilateral
        ) {
            Sender                = sender;
            Receiver              = receiver;
            ExchangesFromSender   = fromSender;
            ExchangesFromReceiver = fromReceiver;
            BilateralExchanges    = bilateral;
        }

        #endregion

        #region instance methods

        #region from IOngoingDeal

        public void Start() {
            foreach(var exchange in ExchangesFromSender) {
                exchange.TerminationRequested += OnExchangeTerminationRequested;
                exchange.Start();
            }

            foreach(var exchange in ExchangesFromReceiver) {
                exchange.TerminationRequested += OnExchangeTerminationRequested;
                exchange.Start();
            }

            foreach(var exchange in BilateralExchanges) {
                exchange.TerminationRequested += OnExchangeTerminationRequested;
                exchange.Start();
            }
        }

        public void End() {
            foreach(var exchange in ExchangesFromSender) {
                exchange.TerminationRequested -= OnExchangeTerminationRequested;
                exchange.End();
            }

            foreach(var exchange in ExchangesFromReceiver) {
                exchange.TerminationRequested -= OnExchangeTerminationRequested;
                exchange.End();
            }

            foreach(var exchange in BilateralExchanges) {
                exchange.TerminationRequested -= OnExchangeTerminationRequested;
                exchange.End();
            }
        }

        #endregion

        private void OnExchangeTerminationRequested(object sender, EventArgs e) {
            if(TerminationRequested != null) {
                TerminationRequested(this, new OngoingDealEventArgs(this));
            }
        }

        #endregion

    }

}
