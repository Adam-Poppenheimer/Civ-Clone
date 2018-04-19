using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.UI;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Diplomacy {

    public class GoldDiplomaticExchange : IDiplomaticExchange {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public bool RequiresIntegerInput {
            get { return true; }
        }

        public int IntegerInput { get; set; }

        #endregion

        private IYieldFormatter YieldFormatter;

        #endregion

        #region constructors

        [Inject]
        public GoldDiplomaticExchange(IYieldFormatter yieldFormatter) {
            YieldFormatter = yieldFormatter;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticRequest

        public bool OverlapsWithExchange(IDiplomaticExchange exchange) {
            return exchange is GoldDiplomaticExchange;
        }

        public bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            return fromCiv.GoldStockpile >= IntegerInput;
        }

        public IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(!CanExecuteBetweenCivs(fromCiv, toCiv)) {
                throw new InvalidOperationException("CanExecuteBetweenCivs must return true on the arguments");
            }

            fromCiv.GoldStockpile -= IntegerInput;
            toCiv  .GoldStockpile += IntegerInput;

            return null;
        }

        public string GetSummary() {
            return "Gold (Lump Sum)";
        }

        #endregion

        #endregion
        
    }

}
