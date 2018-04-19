using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class OngoingDeal : IOngoingDeal {

        #region instance fields and properties

        #region from IOngoingDiplomaticExchange

        public ICivilization Receiver { get; set; }

        public ICivilization Sender { get; set; }

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
            List<IOngoingDiplomaticExchange> fromSender, List<IOngoingDiplomaticExchange> fromReceiver,
            List<IOngoingDiplomaticExchange> bilateral
        ) {
            ExchangesFromSender = fromSender;
            ExchangesFromReceiver = fromReceiver;
            BilateralExchanges  = bilateral;

            foreach(var exchange in fromSender.Concat(fromReceiver).Concat(bilateral)) {
                exchange.TerminationRequested += delegate(object sender, EventArgs e) {
                    if(TerminationRequested != null) {
                        TerminationRequested(this, new OngoingDealEventArgs(this));
                    }
                };
            }
        }

        #endregion

        #region instance methods

        #region from IOngoingDeal

        public void Start() {
            foreach(var exchange in ExchangesFromSender) {
                exchange.Start();
            }

            foreach(var exchange in ExchangesFromReceiver) {
                exchange.Start();
            }

            foreach(var exchange in BilateralExchanges) {
                exchange.Start();
            }
        }

        public void End() {
            foreach(var exchange in ExchangesFromSender) {
                exchange.End();
            }

            foreach(var exchange in ExchangesFromReceiver) {
                exchange.End();
            }

            foreach(var exchange in BilateralExchanges) {
                exchange.End();
            }
        }

        #endregion

        #endregion

    }

}
