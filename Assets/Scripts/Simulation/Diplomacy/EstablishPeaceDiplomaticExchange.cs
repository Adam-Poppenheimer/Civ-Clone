using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class EstablishPeaceDiplomaticExchange : DiplomaticExchangeBase {

        #region instance fields and properties

        #region from DiplomaticExchangeBase

        public override ExchangeType Type {
            get { return ExchangeType.Peace; }
        }

        public override bool RequiresIntegerInput {
            get { return false; }
        }

        #endregion

        private IWarCanon WarCanon;

        #endregion

        #region constructors

        [Inject]
        public EstablishPeaceDiplomaticExchange(IWarCanon warCanon) {
            WarCanon = warCanon;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticExchange

        public override bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            return WarCanon.AreAtWar(fromCiv, toCiv);
        }

        public override IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(!CanExecuteBetweenCivs(fromCiv, toCiv)) {
                throw new InvalidOperationException("CanExecuteBetweenCivs must return true on the given arguments");
            }
            
            WarCanon.EstablishPeace(fromCiv, toCiv);

            return null;
        }

        public override string GetSummary() {
            return "Peace Treaty";
        }

        #endregion

        #endregion
        
    }

}
