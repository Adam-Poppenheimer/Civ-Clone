using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Diplomacy {

    public class ExchangeSummary {

        #region instance fields and properties

        public int MaxOfferableGoldFromSender;

        public int MaxDemandableGoldOfReceiver;

        public List<IDiplomaticExchange> AllPossibleOffersFromSender = new List<IDiplomaticExchange>();

        public List<IDiplomaticExchange> AllPossibleDemandsOfReceiver = new List<IDiplomaticExchange>();

        #endregion

    }

}
