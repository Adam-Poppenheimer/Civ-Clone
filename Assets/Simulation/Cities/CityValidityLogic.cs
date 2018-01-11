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

        private ITilePossessionCanon CellPossessionCanon;

        private IHexGrid HexGrid;

        private ICityFactory CityFactory;

        private ICityConfig Config;

        private IRiverCanon RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public CityValidityLogic(
            ITilePossessionCanon tilePossessionCanon, IHexGrid hexGrid,
            ICityFactory cityFactory, ICityConfig config, IRiverCanon riverCanon
        ){
            CellPossessionCanon = tilePossessionCanon;
            HexGrid             = hexGrid;
            CityFactory         = cityFactory;
            Config              = config;
            RiverCanon          = riverCanon;
        }

        #endregion

        #region instance methods

        #region from ICityValidityLogic

        public bool IsCellValidForCity(IHexCell cell) {
            if(CellPossessionCanon.GetCityOfTile(cell) != null) {
                return false;
            }

            if(cell.IsUnderwater || RiverCanon.HasRiver(cell)) {
                return false;
            }

            foreach(var city in CityFactory.AllCities) {
                if(HexGrid.GetDistance(cell, city.Location) < Config.MinimumSeparation) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #endregion

    }

}
