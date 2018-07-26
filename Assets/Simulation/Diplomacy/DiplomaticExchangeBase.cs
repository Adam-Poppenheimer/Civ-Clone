using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Diplomacy {

    public abstract class DiplomaticExchangeBase : IDiplomaticExchange {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public abstract ExchangeType Type { get; }

        public abstract bool RequiresIntegerInput { get; }

        public int IntegerInput { get; set; }

        public ICity                        CityInput     { get; set; }
        public IResourceDefinition ResourceInput { get; set; }

        #endregion

        #endregion

        #region constructors



        #endregion

        #region instance methods

        #region from IDiplomaticExchange

        public bool OverlapsWithExchange(IDiplomaticExchange exchange) {
            return Type == exchange.Type && (
                (CityInput     != null && CityInput     == exchange.CityInput) ||
                (ResourceInput != null && ResourceInput == exchange.ResourceInput)
            );
        }

        public abstract bool                       CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv);
        public abstract IOngoingDiplomaticExchange ExecuteBetweenCivs   (ICivilization fromCiv, ICivilization toCiv);

        public abstract string GetSummary();

        #endregion

        #endregion
        
    }

}
