using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapManagement {

    public class OngoingDealComposer : IOngoingDealComposer {

        #region instance fields and properties

        private ICivilizationFactory               CivFactory;
        private IOngoingDiplomaticExchangeComposer OngoingExchangeComposer;

        #endregion

        #region constructors

        [Inject]
        public OngoingDealComposer(
            ICivilizationFactory civFactory,
            IOngoingDiplomaticExchangeComposer ongoingExchangeComposer
        ){
            CivFactory              = civFactory;
            OngoingExchangeComposer = ongoingExchangeComposer;
        }

        #endregion

        #region instance methods

        #region from IOngoingDealComposer

        public SerializableOngoingDealData ComposeOngoingDeal(IOngoingDeal ongoingDeal) {
            var retval = new SerializableOngoingDealData();

            retval.Sender    = ongoingDeal.Sender.Name;
            retval.Receiver  = ongoingDeal.Receiver.Name;
            retval.TurnsLeft = ongoingDeal.TurnsLeft;

            foreach(var offer in ongoingDeal.ExchangesFromSender) {
                retval.ExchangesFromSender.Add(OngoingExchangeComposer.ComposeOngoingExchange(offer));
            }

            foreach(var demand in ongoingDeal.ExchangesFromReceiver) {
                retval.ExchangesFromReceiver.Add(OngoingExchangeComposer.ComposeOngoingExchange(demand));
            }

            foreach(var bilateral in ongoingDeal.BilateralExchanges) {
                retval.BilateralExchanges.Add(OngoingExchangeComposer.ComposeOngoingExchange(bilateral));
            }

            return retval;
        }

        public IOngoingDeal DecomposeOngoingDeal(SerializableOngoingDealData ongoingDealData) {
            var sender   = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(ongoingDealData.Sender))  .FirstOrDefault();
            var receiver = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(ongoingDealData.Receiver)).FirstOrDefault();

            if(sender == null) {
                throw new InvalidOperationException("Could not find a sender of the specified name");
            }

            if(receiver == null) {
                throw new InvalidOperationException("Could not find a receiver of the specified name");
            }

            var fromSender   = new List<IOngoingDiplomaticExchange>();
            var fromReceiver = new List<IOngoingDiplomaticExchange>();
            var bilateral    = new List<IOngoingDiplomaticExchange>();

            foreach(var offerData in ongoingDealData.ExchangesFromSender) {
                fromSender.Add(OngoingExchangeComposer.DecomposeOngoingExchange(offerData));
            }

            foreach(var demandData in ongoingDealData.ExchangesFromReceiver) {
                fromReceiver.Add(OngoingExchangeComposer.DecomposeOngoingExchange(demandData));
            }

            foreach(var bilateralData in ongoingDealData.BilateralExchanges) {
                bilateral.Add(OngoingExchangeComposer.DecomposeOngoingExchange(bilateralData));
            }

            var retval = new OngoingDeal(sender, receiver, fromSender, fromReceiver, bilateral);

            retval.TurnsLeft = ongoingDealData.TurnsLeft;

            return retval;
        }

        #endregion

        #endregion

    }

}
