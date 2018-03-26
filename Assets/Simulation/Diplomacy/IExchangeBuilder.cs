using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IExchangeBuilder {

        #region methods

        ExchangeSummary BuildAllValidExchangesBetween(ICivilization sender, ICivilization receiver);

        #endregion

    }

}
