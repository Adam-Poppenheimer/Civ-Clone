using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Diplomacy {

    public class NullExchange : IDiplomaticExchange {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public bool RequiresIntegerInput {
            get { return false; }
        }

        public int IntegerInput { get; set; }

        #endregion

        #endregion

        #region instance methods

        #region from IDiplomaticExchange

        public bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            return true;
        }

        public void ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            
        }

        public bool OverlapsWithExchange(IDiplomaticExchange exchange) {
            return exchange is NullExchange;
        }

        public string GetSummary() {
            return "Null";
        }

        #endregion

        #endregion
        
    }

}
