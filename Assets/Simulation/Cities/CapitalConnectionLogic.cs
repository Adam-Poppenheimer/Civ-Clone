using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities {

    public class CapitalConnectionLogic : ICapitalConnectionLogic {

        #region instance fields and properties

        private ICapitalCityCanon                             CapitalCityCanon;
        private IHexPathfinder                                HexPathfinder;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IConnectionPathCostLogic                      ConnectionPathCostLogic;

        #endregion

        #region constructors

        [Inject]
        public CapitalConnectionLogic(
            ICapitalCityCanon capitalCityCanon, IHexPathfinder hexPathfinder,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IConnectionPathCostLogic connectionPathCostLogic
        ) {
            CapitalCityCanon        = capitalCityCanon;
            HexPathfinder           = hexPathfinder;
            CityPossessionCanon     = cityPossessionCanon;
            CityLocationCanon       = cityLocationCanon;
            ConnectionPathCostLogic = connectionPathCostLogic;
        }

        #endregion

        #region instance methods

        #region from ICapitalConnectionLogic

        public bool IsCityConnectedToCapital(ICity city) {
            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            var capitalOfOwner = CapitalCityCanon.GetCapitalOfCiv(cityOwner);

            if(capitalOfOwner == cityOwner) {
                return true;
            }

            var cityLocation    = CityLocationCanon.GetOwnerOfPossession(city);
            var capitalLocation = CityLocationCanon.GetOwnerOfPossession(capitalOfOwner);

            var pathTo = HexPathfinder.GetShortestPathBetween(
                cityLocation, capitalLocation, ConnectionPathCostLogic.BuildCapitalConnectionPathCostFunction(cityOwner)
            );

            return pathTo != null;
        }

        #endregion

        #endregion
        
    }

}
