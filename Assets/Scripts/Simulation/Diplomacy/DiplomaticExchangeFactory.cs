using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Diplomacy {

    public class DiplomaticExchangeFactory : IDiplomaticExchangeFactory {

        #region instance fields and properties

        private DiContainer Container;

        #endregion

        #region constructors

        [Inject]
        public DiplomaticExchangeFactory(DiContainer container) {
            Container = container;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticExchangeFactory

        public IDiplomaticExchange BuildExchangeForType(ExchangeType type) {
            switch(type) {
                case ExchangeType.City:        return Container.Instantiate<CityDiplomaticExchange>();
                case ExchangeType.GoldLumpSum: return Container.Instantiate<GoldDiplomaticExchange>();
                case ExchangeType.Peace:       return Container.Instantiate<EstablishPeaceDiplomaticExchange>();
                case ExchangeType.Resource:    return Container.Instantiate<ResourceDiplomaticExchange>();
                default: throw new NotImplementedException("BuildExchangeForType does not have a construction plan for the given type");
            }
        }

        public IOngoingDiplomaticExchange BuildOngoingExchangeForType(ExchangeType type) {
            switch(type) {
                case ExchangeType.Resource: return Container.Instantiate<ResourceOngoingDiplomaticExchange>();
                default: throw new NotImplementedException("BuildOngoingExchangeForType does not have a construction plan for the given type");
            }
        }

        #endregion

        #endregion
        
    }

}
