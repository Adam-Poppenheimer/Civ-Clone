using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapManagement {

    public class DiplomaticProposalComposer : IDiplomaticProposalComposer {

        #region instance fields and properties

        private ICivilizationFactory        CivFactory;
        private IDiplomaticExchangeComposer ExchangeComposer;

        #endregion

        #region constructors

        [Inject]
        public DiplomaticProposalComposer(
            ICivilizationFactory civFactory, IDiplomaticExchangeComposer exchangeComposer
        ){
            CivFactory       = civFactory;
            ExchangeComposer = exchangeComposer;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticProposalComposer

        public SerializableProposalData ComposeProposal(IDiplomaticProposal proposal) {
            var retval = new SerializableProposalData();

            retval.Sender   = proposal.Sender.Name;
            retval.Receiver = proposal.Receiver.Name;

            foreach(var offer in proposal.OfferedBySender) {
                retval.OfferedBySender.Add(ExchangeComposer.ComposeExchange(offer));
            }

            foreach(var demand in proposal.DemandedOfReceiver) {
                retval.DemandedOfReceiver.Add(ExchangeComposer.ComposeExchange(demand));
            }

            foreach(var bilateral in proposal.BilateralExchanges) {
                retval.BilateralExchanges.Add(ExchangeComposer.ComposeExchange(bilateral));
            }

            return retval;
        }

        public IDiplomaticProposal DecomposeProposal(SerializableProposalData proposalData) {
            var sender   = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(proposalData.Sender))  .FirstOrDefault();
            var receiver = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(proposalData.Receiver)).FirstOrDefault();

            if(sender == null) {
                throw new InvalidOperationException("Could not find a sender of the specified name");
            }

            if(receiver == null) {
                throw new InvalidOperationException("Could not find a receiver of the specified name");
            }

            var retval = new DiplomaticProposal(sender, receiver);

            foreach(var offerData in proposalData.OfferedBySender) {
                retval.AddAsOffer(ExchangeComposer.DecomposeExchange(offerData));
            }

            foreach(var demandData in proposalData.DemandedOfReceiver) {
                retval.AddAsDemand(ExchangeComposer.DecomposeExchange(demandData));
            }

            foreach(var bilateralData in proposalData.BilateralExchanges) {
                retval.AddAsBilateralExchange(ExchangeComposer.DecomposeExchange(bilateralData));
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
