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

    public class CityValidityLogic : ICityValidityLogic {

        #region instance fields and properties

        private ITilePossessionCanon TilePossessionCanon;

        private IHexGrid HexGrid;

        private ICityFactory CityFactory;

        private ICityConfig Config;

        #endregion

        #region constructors

        [Inject]
        public CityValidityLogic(ITilePossessionCanon tilePossessionCanon, IHexGrid hexGrid,
            ICityFactory cityFactory, ICityConfig config) {

            TilePossessionCanon = tilePossessionCanon;
            HexGrid             = hexGrid;
            CityFactory         = cityFactory;
            Config              = config;
        }

        #endregion

        #region instance methods

        #region from ICityValidityLogic

        public bool IsTileValidForCity(IHexCell tile) {
            if(TilePossessionCanon.GetCityOfTile(tile) != null) {
                return false;
            }

            foreach(var city in CityFactory.AllCities) {
                if(HexGrid.GetDistance(tile, city.Location) < Config.MinimumSeparation) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #endregion

    }

}
