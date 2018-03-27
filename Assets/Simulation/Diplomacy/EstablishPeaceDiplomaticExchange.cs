using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class EstablishPeaceDiplomaticExchange : IDiplomaticExchange {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public int IntegerInput { get; set; }

        public bool RequiresIntegerInput {
            get { return false; }
        }

        #endregion

        private IWarCanon WarCanon;

        #endregion

        #region constructors

        public EstablishPeaceDiplomaticExchange(IWarCanon warCanon) {
            WarCanon = warCanon;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticExchange

        public bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            return WarCanon.AreAtWar(fromCiv, toCiv);
        }

        public void ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(!CanExecuteBetweenCivs(fromCiv, toCiv)) {
                throw new InvalidOperationException("CanExecuteBetweenCivs must return true on the given arguments");
            }
            
            WarCanon.EstablishPeace(fromCiv, toCiv);
        }

        public string GetSummary() {
            return "Peace Treaty";
        }

        public bool OverlapsWithExchange(IDiplomaticExchange exchange) {
            return exchange is EstablishPeaceDiplomaticExchange;
        }

        #endregion

        #endregion
        
    }

}
