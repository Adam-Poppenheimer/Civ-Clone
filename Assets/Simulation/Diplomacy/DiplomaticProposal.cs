using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class DiplomaticProposal : IDiplomaticProposal {

        #region instance fields and properties

        #region from IDiplomaticProposal

        public ICivilization Sender { get; private set; }

        public ICivilization Receiver { get; private set; }

        public IEnumerable<IDiplomaticExchange> OfferedBySender {
            get { return offeredBySender; }
        }
        private HashSet<IDiplomaticExchange> offeredBySender = new HashSet<IDiplomaticExchange>();

        public IEnumerable<IDiplomaticExchange> DemandedOfReceiver {
            get { return demandedOfReceiver; }
        }
        private HashSet<IDiplomaticExchange> demandedOfReceiver = new HashSet<IDiplomaticExchange>();

        #endregion

        #endregion

        #region constructors

        public DiplomaticProposal(ICivilization sender, ICivilization receiver) {
            Sender   = sender;
            Receiver = receiver;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticProposal

        public bool CanAddAsOffer(IDiplomaticExchange exchange) {
            if(exchange == null) {
                throw new ArgumentNullException("request");
            }

            return !HasOverlap(exchange) && exchange.CanExecuteBetweenCivs(Sender, Receiver);
        }

        public void AddAsOffer(IDiplomaticExchange exchange) {
            if(!CanAddAsOffer(exchange)) {
                throw new InvalidOperationException("CanAddOffer must return true on the argued exchange");
            }

            offeredBySender.Add(exchange);
        }

        public void RemoveFromOffers(IDiplomaticExchange exchange) {
            if(exchange == null) {
                throw new ArgumentNullException("request");
            }
            
            offeredBySender.Remove(exchange);
        }

        public bool CanAddAsDemand(IDiplomaticExchange exchange) {
            if(exchange == null) {
                throw new ArgumentNullException("request");
            }

            return !HasOverlap(exchange) && exchange.CanExecuteBetweenCivs(Receiver, Sender);
        }

        public void AddAsDemand(IDiplomaticExchange exchange) {
            if(!CanAddAsDemand(exchange)) {
                throw new InvalidOperationException("CanAddAsDemand must return true on the argued exchange");
            }

            demandedOfReceiver.Add(exchange);
        }

        public void RemoveFromDemands(IDiplomaticExchange exchange) {
            if(exchange == null) {
                throw new ArgumentNullException("request");
            }
            
            demandedOfReceiver.Remove(exchange);
        }

        public bool CanPerformProposal() {
            foreach(var exchange in offeredBySender) {
                if(!exchange.CanExecuteBetweenCivs(Sender, Receiver)) {
                    return false;
                }
            }

            foreach(var exchange in demandedOfReceiver) {
                if(!exchange.CanExecuteBetweenCivs(Receiver, Sender)) {
                    return false;
                }
            }

            return true;
        }

        public void PerformProposal() {
            if(!CanPerformProposal()) {
                throw new InvalidOperationException("CanPerformProposal must return true");
            }

            foreach(var offer in offeredBySender) {
                offer.ExecuteBetweenCivs(Sender, Receiver);
            }

            foreach(var demand in demandedOfReceiver) {
                demand.ExecuteBetweenCivs(Receiver, Sender);
            }
        }

        #endregion

        private bool HasOverlap(IDiplomaticExchange exchange) {
            foreach(var existingOffer in OfferedBySender) {
                if(existingOffer.OverlapsWithExchange(exchange)) {
                    return true;
                }
            }

            foreach(var existingDemand in DemandedOfReceiver) {
                if(existingDemand.OverlapsWithExchange(exchange)) {
                    return true;
                }
            }

            return false;
        }

        #endregion

    }

}
