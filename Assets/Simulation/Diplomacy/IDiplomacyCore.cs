using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IDiplomacyCore {

        #region methods

        IEnumerable<IDiplomaticProposal> GetProposalsReceivedByCiv(ICivilization civ);

        IEnumerable<IDiplomaticProposal> GetProposalsSentFromCiv(ICivilization civ);

        bool TryAcceptProposal(IDiplomaticProposal proposal);

        void RejectProposal(IDiplomaticProposal proposal);

        void SendProposal(IDiplomaticProposal proposal);



        IEnumerable<IOngoingDeal> GetOngoingDealsReceivedByCiv(ICivilization civ);

        IEnumerable<IOngoingDeal> GetOngoingDealsSentFromCiv(ICivilization civ);

        void SubscribeOngoingDeal(IOngoingDeal exchange);

        void UnsubscribeOngoingDeal(IOngoingDeal exchange);

        void UpdateOngoingDeals();

        #endregion

    }

}
