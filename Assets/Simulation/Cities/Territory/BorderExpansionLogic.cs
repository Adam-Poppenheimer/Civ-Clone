using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Simulation.Cities.Territory {

    /// <summary>
    /// The standard implementation of IBorderExpansionLogic.
    /// </summary>
    public class BorderExpansionLogic : IBorderExpansionLogic {

        #region instance fields and properties

        private IHexGrid                                 HexGrid;
        private IPossessionRelationship<ICity, IHexCell> PossessionCanon;        
        private ICityConfig                              Config;
        private IYieldGenerationLogic                    YieldGenerationLogic;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hexGrid"></param>
        /// <param name="possessionCanon"></param>
        /// <param name="config"></param>
        /// <param name="resourceGenerationLogic"></param>
        [Inject]
        public BorderExpansionLogic(
            IHexGrid hexGrid, IPossessionRelationship<ICity, IHexCell> possessionCanon,
            ICityConfig config, IYieldGenerationLogic resourceGenerationLogic,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            HexGrid                 = hexGrid;
            PossessionCanon         = possessionCanon;
            Config                  = config;
            YieldGenerationLogic = resourceGenerationLogic;
            CityLocationCanon       = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICulturalExpansionLogic

        /// <inheritdoc/>
        public IEnumerable<IHexCell> GetAllCellsAvailableToCity(ICity city) {
            var retval = new HashSet<IHexCell>();

            foreach(var tile in PossessionCanon.GetPossessionsOfOwner(city)) {
                foreach(var neighbor in HexGrid.GetNeighbors(tile)) {

                    if(IsCellAvailable(city, neighbor)) {
                        retval.Add(neighbor);
                    }

                }
            }

            return retval;
        }

        /// <inheritdoc/>
        public IHexCell GetNextCellToPursue(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }
            var availableTiles = GetAllCellsAvailableToCity(city).ToList();

            availableTiles.Sort(CellComparisonUtil.BuildComparisonAscending(city, city.YieldFocus, YieldGenerationLogic));

            return availableTiles.LastOrDefault();
        }

        /// <inheritdoc/>
        public bool IsCellAvailable(ICity city, IHexCell cell) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(cell == null){
                throw new ArgumentNullException("cell");
            }

            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            bool isUnowned = PossessionCanon.GetOwnerOfPossession(cell) == null;
            bool isWithinRange = HexGrid.GetDistance(cityLocation, cell) <= Config.MaxBorderRange;
            bool isNeighborOfPossession = HexGrid.GetNeighbors(cell).Exists(neighbor => PossessionCanon.GetOwnerOfPossession(neighbor) == city);

            return isUnowned && isWithinRange && isNeighborOfPossession;
        }

        /// <inheritdoc/>
        public int GetCultureCostOfAcquiringCell(ICity city, IHexCell tile) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }else if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            var tileCount = PossessionCanon.GetPossessionsOfOwner(city).Count();

            return Mathf.FloorToInt(
                Config.CellCostBase + 
                Mathf.Pow(
                    Config.PreviousCellCountCoefficient * (tileCount - 1),
                    Config.PreviousCellCountExponent
                )
            );
        }

        /// <inheritdoc/>
        public int GetGoldCostOfAcquiringCell(ICity city, IHexCell tile) {
            return GetCultureCostOfAcquiringCell(city, tile);
        }

        #endregion

        #endregion
        
    }

}
