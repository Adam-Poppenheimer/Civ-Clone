using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class ResourceDiplomaticExchange : IDiplomaticExchange {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public int IntegerInput { set; get; }

        public bool RequiresIntegerInput { get; set; }

        #endregion

        public ISpecialtyResourceDefinition ResourceToExchange { get; set; }



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

        public bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            return ResourceTransferCanon.CanExportCopiesOfResource(ResourceToExchange, IntegerInput, fromCiv, toCiv);
        }

        public IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(!CanExecuteBetweenCivs(fromCiv, toCiv)) {
                throw new InvalidOperationException("CanExecuteBetweenCivs must return true for the given arguments");
            }

            var ongoingExchange = Container.Instantiate<ResourceOngoingDiplomaticExchange>();

            ongoingExchange.ResourceToTransfer = ResourceToExchange;
            ongoingExchange.CopiesToTransfer   = IntegerInput;
            ongoingExchange.Exporter           = fromCiv;
            ongoingExchange.Importer           = toCiv;

            return ongoingExchange;
        }

        public string GetSummary() {
            return ResourceToExchange.name;
        }

        public bool OverlapsWithExchange(IDiplomaticExchange exchange) {
            var otherResourceExchange = exchange as ResourceDiplomaticExchange;
            return otherResourceExchange != null && otherResourceExchange.ResourceToExchange == ResourceToExchange;
        }

        #endregion

        #endregion
        
    }

}
