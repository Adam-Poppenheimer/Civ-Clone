using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapManagement {

    public class OngoingDiplomaticExchangeComposer : IOngoingDiplomaticExchangeComposer {

        #region instance fields and properties

        private ICivilizationFactory                      CivFactory;
        private IDiplomaticExchangeFactory                ExchangeFactory;
        private IEnumerable<IResourceDefinition> AvailableResources;

        #endregion

        #region constructors

        [Inject]
        public OngoingDiplomaticExchangeComposer(
            ICivilizationFactory civFactory, IDiplomaticExchangeFactory exchangeFactory,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ) {
            CivFactory         = civFactory;
            ExchangeFactory    = exchangeFactory;
            AvailableResources = availableResources;
        }

        #endregion

        #region instance methods

        #region from IOngoingDiplomaticExchangeComposer

        public SerializableOngoingDiplomaticExchange ComposeOngoingExchange(IOngoingDiplomaticExchange ongoingExchange) {
            var retval = new SerializableOngoingDiplomaticExchange();

            retval.Type          = ongoingExchange.Type;
            retval.Sender        = ongoingExchange.Sender       .Name;
            retval.Receiver      = ongoingExchange.Receiver     .Name;
            retval.ResourceInput = ongoingExchange.ResourceInput.name;
            retval.IntInput      = ongoingExchange.IntegerInput;

            return retval;
        }

        public IOngoingDiplomaticExchange DecomposeOngoingExchange(SerializableOngoingDiplomaticExchange ongoingData) {
            var retval = ExchangeFactory.BuildOngoingExchangeForType(ongoingData.Type);

            var sender   = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(ongoingData.Sender))  .FirstOrDefault();
            var receiver = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(ongoingData.Receiver)).FirstOrDefault();

            if(sender == null) {
                throw new InvalidOperationException("Could not find a sender of the specified name");
            }

            if(receiver == null) {
                throw new InvalidOperationException("Could not find a receiver of the specified name");
            }

            retval.Sender   = sender;
            retval.Receiver = receiver;
            retval.IntegerInput = ongoingData.IntInput;

            if(ongoingData.ResourceInput != null) {
                var resourceInput = AvailableResources.Where(resource => resource.name.Equals(ongoingData.ResourceInput)).FirstOrDefault();

                if(resourceInput == null) {
                    throw new InvalidOperationException("Could not find a resource with the specified name");
                }

                retval.ResourceInput = resourceInput;
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
