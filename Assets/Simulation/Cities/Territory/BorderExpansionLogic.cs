﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Simulation.Cities.Territory {

    public class BorderExpansionLogic : IBorderExpansionLogic {

        #region instance fields and properties

        private IHexGrid HexGrid;

        private ITilePossessionCanon PossessionCanon;
        
        private ICityConfig Config;

        private IResourceGenerationLogic ResourceGenerationLogic;

        #endregion

        #region constructors

        [Inject]
        public BorderExpansionLogic(IHexGrid hexGrid, ITilePossessionCanon possessionCanon,
            ICityConfig config, IResourceGenerationLogic resourceGenerationLogic) {

            HexGrid = hexGrid;
            PossessionCanon = possessionCanon;
            Config = config;
            ResourceGenerationLogic = resourceGenerationLogic;

        }

        #endregion

        #region instance methods

        #region from ICulturalExpansionLogic

        public IEnumerable<IHexCell> GetAllTilesAvailableToCity(ICity city) {
            var retval = new HashSet<IHexCell>();

            foreach(var tile in PossessionCanon.GetTilesOfCity(city)) {
                foreach(var neighbor in HexGrid.GetNeighbors(tile)) {

                    if(IsTileAvailable(city, neighbor)) {
                        retval.Add(neighbor);
                    }

                }
            }

            return retval;
        }

        public IHexCell GetNextTileToPursue(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }
            var availableTiles = GetAllTilesAvailableToCity(city).ToList();

            availableTiles.Sort(TileComparisonUtil.BuildComparisonAscending(city, city.ResourceFocus, ResourceGenerationLogic));

            return availableTiles.LastOrDefault();
        }

        public bool IsTileAvailable(ICity city, IHexCell tile) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(tile == null){
                throw new ArgumentNullException("tile");
            }

            bool isUnowned = PossessionCanon.GetCityOfTile(tile) == null;
            bool isWithinRange = HexGrid.GetDistance(city.Location, tile) <= Config.MaxBorderRange;
            bool isNeighborOfPossession = HexGrid.GetNeighbors(tile).Exists(neighbor => PossessionCanon.GetCityOfTile(neighbor) == city);

            return isUnowned && isWithinRange && isNeighborOfPossession;
        }

        public int GetCultureCostOfAcquiringTile(ICity city, IHexCell tile) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            var tileCount = PossessionCanon.GetTilesOfCity(city).Count();

            return Mathf.FloorToInt(
                Config.TileCostBase + 
                Mathf.Pow(
                    Config.PreviousTileCountCoefficient * (tileCount - 1),
                    Config.PreviousTileCountExponent
                )
            );
        }

        public int GetGoldCostOfAcquiringTile(ICity city, IHexCell tile) {
            return GetCultureCostOfAcquiringTile(city, tile);
        }

        #endregion

        #endregion
        
    }

}
