using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IDiplomaticProposal {

        #region properties

        ICivilization Sender { get; }

        ICivilization Receiver { get; }

        IEnumerable<IDiplomaticExchange> OfferedBySender { get; }

        IEnumerable<IDiplomaticExchange> DemandedOfReceiver { get; }

        IEnumerable<IDiplomaticExchange> BilateralExchanges { get; }

        #endregion

        #region methods

        bool CanAddAsOffer   (IDiplomaticExchange exchange);
        void AddAsOffer      (IDiplomaticExchange exchange);
        void RemoveFromOffers(IDiplomaticExchange exchange);

        bool CanAddAsDemand   (IDiplomaticExchange exchange);
        void AddAsDemand      (IDiplomaticExchange exchange);
        void RemoveFromDemands(IDiplomaticExchange exchange);

        bool CanAddAsBilateralExchange   (IDiplomaticExchange exchange);
        void AddAsBilateralExchange      (IDiplomaticExchange exchange);
        void RemoveFromBilateralExchanges(IDiplomaticExchange exchange);

        bool CanPerformProposal();

        void PerformProposal();
        
        #endregion

    }

}
