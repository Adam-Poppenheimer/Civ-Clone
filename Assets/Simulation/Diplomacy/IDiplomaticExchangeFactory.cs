using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Diplomacy {

    public interface IDiplomaticExchangeFactory {

        #region methods

        IDiplomaticExchange BuildExchangeForType(ExchangeType type);

        IOngoingDiplomaticExchange BuildOngoingExchangeForType(ExchangeType type);

        #endregion

    }

}
