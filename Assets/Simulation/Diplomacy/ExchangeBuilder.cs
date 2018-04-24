using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Diplomacy {

    public class ExchangeBuilder : IExchangeBuilder {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IWarCanon                                     WarCanon;
        private DiContainer                                   Container;
        private IResourceExchangeBuilder                      ResourceExchangeBuilder;

        #endregion

        #region constructors

        [Inject]
        public ExchangeBuilder(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IWarCanon warCanon, DiContainer container,
            IResourceExchangeBuilder resourceExchangeBuilder
        ){
            CityPossessionCanon         = cityPossessionCanon;
            WarCanon                    = warCanon;
            Container                   = container;
            ResourceExchangeBuilder     = resourceExchangeBuilder;
        }

        #endregion

        #region instance methods

        #region from IExchangeBuilder

        public ExchangeSummary BuildAllValidExchangesBetween(ICivilization sender, ICivilization receiver) {
            var retval = new ExchangeSummary();

            if(WarCanon.CanEstablishPeace(sender, receiver)) {
                retval.BilateralExchanges.Add(Container.Instantiate<EstablishPeaceDiplomaticExchange>());
            }

            if(sender.GoldStockpile > 0) {
                retval.AllPossibleOffersFromSender.Add(Container.Instantiate<GoldDiplomaticExchange>());
                retval.MaxOfferableGoldFromSender = sender.GoldStockpile;
            }

            if(receiver.GoldStockpile > 0) {
                retval.AllPossibleDemandsOfReceiver.Add(Container.Instantiate<GoldDiplomaticExchange>());
                retval.MaxDemandableGoldOfReceiver = receiver.GoldStockpile;
            }

            BuildCityExchanges(sender, receiver, retval);

            ResourceExchangeBuilder.BuildResourceExchanges(sender, receiver, retval);

            return retval;
        }

        #endregion

        private void BuildCityExchanges(
            ICivilization sender, ICivilization receiver, ExchangeSummary summary
        ){
            foreach(var cityOfSender in CityPossessionCanon.GetPossessionsOfOwner(sender)) {
                var cityOffer = Container.Instantiate<CityDiplomaticExchange>();

                cityOffer.CityInput = cityOfSender;

                summary.AllPossibleOffersFromSender.Add(cityOffer);
            }

            foreach(var cityOfReceiver in CityPossessionCanon.GetPossessionsOfOwner(receiver)) {
                var cityOffer = Container.Instantiate<CityDiplomaticExchange>();

                cityOffer.CityInput = cityOfReceiver;

                summary.AllPossibleDemandsOfReceiver.Add(cityOffer);
            }
        }

        #endregion
        
    }

}
