using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Simulation.Cities.Territory {

    public class BorderExpansionLogic : IBorderExpansionLogic {

        #region instance fields and properties

        private IMapHexGrid HexGrid;

        private ITilePossessionCanon PossessionCanon;
        
        private IBorderExpansionConfig Config;

        private IResourceGenerationLogic ResourceGenerationLogic;

        #endregion

        #region constructors

        [Inject]
        public BorderExpansionLogic(IMapHexGrid hexGrid, ITilePossessionCanon possessionCanon,
            IBorderExpansionConfig config, IResourceGenerationLogic resourceGenerationLogic) {

            HexGrid = hexGrid;
            PossessionCanon = possessionCanon;
            Config = config;
            ResourceGenerationLogic = resourceGenerationLogic;

        }

        #endregion

        #region instance methods

        #region from ICulturalExpansionLogic

        public IEnumerable<IMapTile> GetAllTilesAvailableToCity(ICity city) {
            var retval = new HashSet<IMapTile>();

            foreach(var tile in PossessionCanon.GetTilesOfCity(city)) {
                foreach(var neighbor in HexGrid.GetNeighbors(tile)) {

                    if(IsTileAvailable(city, neighbor)) {
                        retval.Add(neighbor);
                    }

                }
            }

            return retval;
        }

        public IMapTile GetNextTileToPursue(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }
            var availableTiles = GetAllTilesAvailableToCity(city).ToList();

            if(city.DistributionPreferences.ShouldFocusResource) {
                availableTiles.Sort(TileComparisonUtil.BuildFocusedComparisonAscending(
                    city, city.DistributionPreferences.FocusedResource, ResourceGenerationLogic
                ));
            }else {
                availableTiles.Sort(TileComparisonUtil.BuildTotalYieldComparisonAscending(city, ResourceGenerationLogic));
            }

            return availableTiles.LastOrDefault();
        }

        public bool IsTileAvailable(ICity city, IMapTile tile) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(tile == null){
                throw new ArgumentNullException("tile");
            }

            bool isUnowned = PossessionCanon.GetCityOfTile(tile) == null;
            bool isWithinRange = HexGrid.GetDistance(city.Location, tile) <= Config.MaxRange;
            bool isNeighborOfPossession = HexGrid.GetNeighbors(tile).Exists(neighbor => PossessionCanon.GetCityOfTile(neighbor) == city);

            return isUnowned && isWithinRange && isNeighborOfPossession;
        }

        public int GetCultureCostOfAcquiringTile(ICity city, IMapTile tile) {
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

        public int GetGoldCostOfAcquiringTile(ICity city, IMapTile tile) {
            return GetCultureCostOfAcquiringTile(city, tile);
        }

        #endregion

        #endregion
        
    }

}
