using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Diplomacy {

    public interface IDiplomaticExchange {

        #region properties

        ExchangeType Type { get; }

        bool RequiresIntegerInput { get; }

        int IntegerInput { get; set; }

        ICity                        CityInput     { get; set; }
        IResourceDefinition ResourceInput { get; set; }

        #endregion

        #region methods

        bool OverlapsWithExchange(IDiplomaticExchange exchange);

        bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv);

        IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv);

        string GetSummary();

        #endregion

    }

}
