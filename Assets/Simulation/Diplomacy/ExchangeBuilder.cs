using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class ExchangeBuilder : IExchangeBuilder {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private DiContainer Container;

        #endregion

        #region constructors

        [Inject]
        public ExchangeBuilder(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            DiContainer container
        ){
            CityPossessionCanon = cityPossessionCanon;
            Container           = container;
        }

        #endregion

        #region instance methods

        #region from IExchangeBuilder

        public ExchangeSummary BuildAllValidExchangesBetween(ICivilization sender, ICivilization receiver) {
            var retval = new ExchangeSummary();

            if(sender.GoldStockpile > 0) {
                retval.AllPossibleOffersFromSender.Add(Container.Instantiate<GoldDiplomaticExchange>());
            }

            if(receiver.GoldStockpile > 0) {
                retval.AllPossibleDemandsOfReceiver.Add(Container.Instantiate<GoldDiplomaticExchange>());
            }

            BuildCityExchanges(sender, receiver, retval);

            return retval;
        }

        #endregion

        private void BuildCityExchanges(
            ICivilization sender, ICivilization receiver, ExchangeSummary summary
        ){
            foreach(var cityOfSender in CityPossessionCanon.GetPossessionsOfOwner(sender)) {
                var cityOffer = Container.Instantiate<CityDiplomaticExchange>();

                cityOffer.CityToExchange = cityOfSender;

                summary.AllPossibleOffersFromSender.Add(cityOffer);
            }

            foreach(var cityOfReceiver in CityPossessionCanon.GetPossessionsOfOwner(receiver)) {
                var cityOffer = Container.Instantiate<CityDiplomaticExchange>();

                cityOffer.CityToExchange = cityOfReceiver;

                summary.AllPossibleDemandsOfReceiver.Add(cityOffer);
            }
        }

        #endregion
        
    }

}
