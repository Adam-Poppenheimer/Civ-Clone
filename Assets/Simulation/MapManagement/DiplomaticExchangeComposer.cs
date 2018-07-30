using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapManagement {

    public class DiplomaticExchangeComposer : IDiplomaticExchangeComposer {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, ICity>  CityLocationCanon;
        private IDiplomaticExchangeFactory                ExchangeFactory;
        private IHexGrid                                  Grid;
        private IEnumerable<IResourceDefinition> AvailableResources;

        #endregion

        #region constructors

        [Inject]
        public DiplomaticExchangeComposer(
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IDiplomaticExchangeFactory exchangeFactory,
            IHexGrid grid,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ){
            CityLocationCanon  = cityLocationCanon;
            ExchangeFactory    = exchangeFactory;
            Grid               = grid;
            AvailableResources = availableResources;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticExchangeComposer

        public SerializableDiplomaticExchangeData ComposeExchange(IDiplomaticExchange exchange) {
            var retval = new SerializableDiplomaticExchangeData();

            retval.Type         = exchange.Type;
            retval.IntegerInput = exchange.IntegerInput;

            if(exchange.CityInput != null) {
                var cityLocation = CityLocationCanon.GetOwnerOfPossession(exchange.CityInput);
                retval.CityInputLocation = cityLocation.Coordinates;
            }
            
            if(exchange.ResourceInput != null) {
                retval.ResourceInput = exchange.ResourceInput.name;
            }

            return retval;
        }

        public IDiplomaticExchange DecomposeExchange(SerializableDiplomaticExchangeData exchangeData) {
            var retval = ExchangeFactory.BuildExchangeForType(exchangeData.Type);

            retval.IntegerInput = exchangeData.IntegerInput;

            if(exchangeData.CityInputLocation != null) {
                var cellAtCoords = Grid.GetCellAtCoordinates(exchangeData.CityInputLocation);

                if(cellAtCoords == null) {
                    throw new InvalidOperationException("Could not find a cell at the specified coordinates");
                }

                var cityAtLocation = CityLocationCanon.GetPossessionsOfOwner(cellAtCoords).FirstOrDefault();

                if(cityAtLocation == null) {
                    throw new InvalidOperationException("Could not find a city at the specified location");
                }

                retval.CityInput = cityAtLocation;
            }

            if(exchangeData.ResourceInput != null) {
                var resourceOfName = AvailableResources.Where(resource => resource.name.Equals(exchangeData.ResourceInput)).FirstOrDefault();

                if(resourceOfName == null) {
                    throw new InvalidOperationException("Could not find the resource of the specified name");
                }

                retval.ResourceInput = resourceOfName;
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
