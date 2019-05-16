using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// The standard implementation of ICityValidityLogic
    /// </summary>
    public class CityValidityLogic : ICityValidityLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;
        private IHexGrid                                 HexGrid;
        private ICityFactory                             CityFactory;
        private ICityConfig                              Config;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CityValidityLogic(
            IPossessionRelationship<ICity, IHexCell> tilePossessionCanon,
             IHexGrid hexGrid, ICityFactory cityFactory, ICityConfig config,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            CellPossessionCanon = tilePossessionCanon;
            HexGrid             = hexGrid;
            CityFactory         = cityFactory;
            Config              = config;
            CityLocationCanon   = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICityValidityLogic

        public bool IsCellValidForCity(IHexCell cell) {
            if(CellPossessionCanon.GetOwnerOfPossession(cell) != null) {
                return false;
            }

            if(cell.Terrain.IsWater()) {
                return false;
            }

            if(cell.Feature != CellFeature.None) {
                return false;
            }

            foreach(var city in CityFactory.AllCities) {
                var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

                if(HexGrid.GetDistance(cell, cityLocation) < Config.MinimumSeparation) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #endregion

    }

}
