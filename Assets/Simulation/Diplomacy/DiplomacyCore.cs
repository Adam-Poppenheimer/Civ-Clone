using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class DiplomacyCore : IDiplomacyCore {

        #region instance fields and properties

        private List<IDiplomaticProposal> ActiveProposals = new List<IDiplomaticProposal>();

        private List<IOngoingDeal> ActiveDeals = new List<IOngoingDeal>();



        private IDiplomacyConfig Config;

        #endregion

        #region constructors

        [Inject]
        public DiplomacyCore(IDiplomacyConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from IDiplomacyCore

        public IEnumerable<IDiplomaticProposal> GetProposalsSentFromCiv(ICivilization civ) {
            return ActiveProposals.Where(proposal => proposal.Sender == civ);
        }

        public IEnumerable<IDiplomaticProposal> GetProposalsReceivedByCiv(ICivilization civ) {
            return ActiveProposals.Where(proposal => proposal.Receiver == civ);
        }

         public bool TryAcceptProposal(IDiplomaticProposal proposal) {
            if(proposal == null) {
                throw new ArgumentNullException("proposal");
            }
            if(proposal.CanPerformProposal()) {
                var ongoingDeal = proposal.PerformProposal();

                if(ongoingDeal != null) {
                    SubscribeOngoingDeal(ongoingDeal);
                }

                ActiveProposals.Remove(proposal);
                return true;
            }else {
                return false;
            }
        }

        public void RejectProposal(IDiplomaticProposal proposal) {
            if(proposal == null) {
                throw new ArgumentNullException("proposal");
            }
            ActiveProposals.Remove(proposal);
        }

        public void SendProposal(IDiplomaticProposal proposal) {
            if(proposal == null) {
                throw new ArgumentNullException("proposal");
            }
            ActiveProposals.Add(proposal);
        }



        public IEnumerable<IOngoingDeal> GetOngoingDealsReceivedByCiv(ICivilization civ) {
            return ActiveDeals.Where(deal => deal.Receiver == civ);
        }

        public IEnumerable<IOngoingDeal> GetOngoingDealsSentFromCiv(ICivilization civ) {
            return ActiveDeals.Where(deal => deal.Sender == civ);
        }

        public void SubscribeOngoingDeal(IOngoingDeal deal) {
            deal.Start();
            
            deal.TurnsLeft = Config.TradeDuration;

            deal.TerminationRequested += HandleTerminationRequest;

            ActiveDeals.Add(deal);
        }

        public void UnsubscribeOngoingDeal(IOngoingDeal deal) {
            deal.End();

            deal.TerminationRequested -= HandleTerminationRequest;

            ActiveDeals.Remove(deal);
        }

        public void UpdateOngoingDeals() {
            foreach(var deal in new List<IOngoingDeal>(ActiveDeals)) {
                if(--deal.TurnsLeft <= 0) {
                    UnsubscribeOngoingDeal(deal);
                }
            }
        }

        #endregion

        private void HandleTerminationRequest(object sender, OngoingDealEventArgs args) {
            UnsubscribeOngoingDeal(args.Deal);
        }

        #endregion
        
    }

}
