using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Diplomacy {

    public class ResourceExchangeBuilder : IResourceExchangeBuilder {

        #region instance fields and properties
                
        private ICivilizationConnectionLogic              CivilizationConnectionLogic;
        private DiContainer                               Container;
        private IResourceTransferCanon                    ResourceTransferCanon;
        private IFreeResourcesLogic                       FreeResourcesLogic;
        private IEnumerable<IResourceDefinition> AvailableResources;        

        #endregion

        #region constructors

        [Inject]
        public ResourceExchangeBuilder(
            ICivilizationConnectionLogic civilizationConnectionLogic, DiContainer container,
            IResourceTransferCanon resourceTransferCanon, IFreeResourcesLogic freeResourcesLogic,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ) {
            CivilizationConnectionLogic = civilizationConnectionLogic;
            Container                   = container;
            ResourceTransferCanon       = resourceTransferCanon;
            FreeResourcesLogic          = freeResourcesLogic;
            AvailableResources          = availableResources;
        }

        #endregion

        #region instance methods

        #region from IResourceExchangeBuilder

        public void BuildResourceExchanges(
            ICivilization sender, ICivilization receiver, ExchangeSummary summary
        ) {
            if(sender == null) {
                throw new ArgumentNullException("sender");
            }

            if(receiver == null) {
                throw new ArgumentNullException("receiver");
            }

            if(!CivilizationConnectionLogic.AreCivilizationsConnected(sender, receiver)) {
                return;
            }

            var tradeableOfSender = AvailableResources.Where(
                resource => ResourceTransferCanon.GetTradeableCopiesOfResourceForCiv(resource, sender) > 0
            ).ToList();

            var tradeableOfReceiver = AvailableResources.Where(
                resource => ResourceTransferCanon.GetTradeableCopiesOfResourceForCiv(resource, receiver) > 0
            ).ToList();

            var tradeableLuxuriesOfSender   = tradeableOfSender
                .Where(resource => resource.Type == ResourceType.Luxury)
                .Where(resource => FreeResourcesLogic.GetFreeCopiesOfResourceForCiv(resource, receiver) <= 0);

            var tradeableStrategicsOfSender = tradeableOfSender.Where(resource => resource.Type == MapResources.ResourceType.Strategic);

            var tradeableLuxuriesOfReceiver   = tradeableOfReceiver
                .Where(resource => resource.Type == ResourceType.Luxury)
                .Where(resource => FreeResourcesLogic.GetFreeCopiesOfResourceForCiv(resource, sender) <= 0);

            var tradeableStrategicsOfReceiver = tradeableOfReceiver.Where(resource => resource.Type == MapResources.ResourceType.Strategic);

            foreach(var luxury in tradeableLuxuriesOfSender) {
                var luxuryExchange = Container.Instantiate<ResourceDiplomaticExchange>();

                luxuryExchange.ResourceInput = luxury;
                luxuryExchange.IntegerInput = 1;

                summary.AllPossibleOffersFromSender.Add(luxuryExchange);
            }

            foreach(var strategic in tradeableStrategicsOfSender) {
                var strategicExchange = Container.Instantiate<ResourceDiplomaticExchange>();

                strategicExchange.ResourceInput = strategic;
                strategicExchange.IntegerInput = ResourceTransferCanon.GetTradeableCopiesOfResourceForCiv(strategic, sender);

                summary.AllPossibleOffersFromSender.Add(strategicExchange);
            }

            foreach(var luxury in tradeableLuxuriesOfReceiver) {
                var luxuryExchange = Container.Instantiate<ResourceDiplomaticExchange>();

                luxuryExchange.ResourceInput = luxury;
                luxuryExchange.IntegerInput = 1;

                summary.AllPossibleDemandsOfReceiver.Add(luxuryExchange);
            }

            foreach(var strategic in tradeableStrategicsOfReceiver) {
                var strategicExchange = Container.Instantiate<ResourceDiplomaticExchange>();

                strategicExchange.ResourceInput = strategic;
                strategicExchange.IntegerInput = ResourceTransferCanon.GetTradeableCopiesOfResourceForCiv(strategic, receiver);

                summary.AllPossibleDemandsOfReceiver.Add(strategicExchange);
            }
        }

        #endregion

        #endregion

    }

}
