using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.UI;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Diplomacy {

    public class GoldDiplomaticExchange : DiplomaticExchangeBase {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public override ExchangeType Type {
            get { return ExchangeType.GoldLumpSum; }
        }

        public override bool RequiresIntegerInput {
            get { return true; }
        }

        #endregion

        #endregion

        #region constructors

        [Inject]
        public GoldDiplomaticExchange() {
            
        }

        #endregion

        #region instance methods

        #region from IDiplomaticRequest

        public override bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            return fromCiv.GoldStockpile >= IntegerInput;
        }

        public override IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(!CanExecuteBetweenCivs(fromCiv, toCiv)) {
                throw new InvalidOperationException("CanExecuteBetweenCivs must return true on the arguments");
            }

            fromCiv.GoldStockpile -= IntegerInput;
            toCiv  .GoldStockpile += IntegerInput;

            return null;
        }

        public  override string GetSummary() {
            return "Gold (Lump Sum)";
        }

        #endregion

        #endregion
        
    }

}
