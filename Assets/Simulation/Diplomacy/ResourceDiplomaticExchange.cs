using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class ResourceDiplomaticExchange : DiplomaticExchangeBase {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public override ExchangeType Type {
            get { return ExchangeType.Resource; }
        }

        public override bool RequiresIntegerInput {
            get { return ResourceInput.Type == MapResources.ResourceType.Strategic; }
        }

        #endregion



        private IResourceTransferCanon ResourceTransferCanon;
        private DiContainer            Container;

        #endregion

        #region constructors

        [Inject]
        public ResourceDiplomaticExchange(
            IResourceTransferCanon resourceTransferCanon, DiContainer container
        ){
            ResourceTransferCanon = resourceTransferCanon;
            Container             = container;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticExchange

        public override bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            return ResourceTransferCanon.CanExportCopiesOfResource(ResourceInput, IntegerInput, fromCiv, toCiv);
        }

        public override IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(!CanExecuteBetweenCivs(fromCiv, toCiv)) {
                throw new InvalidOperationException("CanExecuteBetweenCivs must return true for the given arguments");
            }

            var ongoingExchange = Container.Instantiate<ResourceOngoingDiplomaticExchange>();

            ongoingExchange.ResourceInput = ResourceInput;
            ongoingExchange.IntegerInput  = IntegerInput;
            ongoingExchange.Sender        = fromCiv;
            ongoingExchange.Receiver      = toCiv;

            return ongoingExchange;
        }

        public override string GetSummary() {
            return ResourceInput.name;
        }

        #endregion

        #endregion
        
    }

}
