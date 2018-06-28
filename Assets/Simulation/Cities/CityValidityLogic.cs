﻿using System;
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

        private IHexGrid HexGrid;

        private ICityFactory CityFactory;

        private ICityConfig Config;

        private IRiverCanon RiverCanon;

        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        /// <summary>
        /// The standard constructor for a CityValidityLogic object.
        /// </summary>
        /// <param name="tilePossessionCanon"></param>
        /// <param name="hexGrid"></param>
        /// <param name="cityFactory"></param>
        /// <param name="config"></param>
        /// <param name="riverCanon"></param>
        [Inject]
        public CityValidityLogic(
            IPossessionRelationship<ICity, IHexCell> tilePossessionCanon, IHexGrid hexGrid,
            ICityFactory cityFactory, ICityConfig config, IRiverCanon riverCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            CellPossessionCanon = tilePossessionCanon;
            HexGrid             = hexGrid;
            CityFactory         = cityFactory;
            Config              = config;
            RiverCanon          = riverCanon;
            CityLocationCanon   = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICityValidityLogic

        /// <inheritdoc/>
        /// <remarks>
        /// Check CityValidityLogicTests to learn exactly what constitutes a valid city placement.
        /// </remarks>
        public bool IsCellValidForCity(IHexCell cell) {
            if(CellPossessionCanon.GetOwnerOfPossession(cell) != null) {
                return false;
            }

            if(cell.Terrain.IsWater() || RiverCanon.HasRiver(cell)) {
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
