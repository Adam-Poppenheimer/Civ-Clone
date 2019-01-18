using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities;

namespace Assets.Simulation.MapGeneration {

    public class StartingUnitPlacementLogic : IStartingUnitPlacementLogic {

        #region instance fields and properties

        private IHexGrid           Grid;
        private IUnitFactory       UnitFactory;
        private ICellScorer        CellScorer;
        private IUnitPositionCanon UnitPositionCanon;
        private ICityConfig        CityConfig;

        #endregion

        #region constructors

        [Inject]
        public StartingUnitPlacementLogic(
            IHexGrid grid, IUnitFactory unitFactory, ICellScorer cellScorer,
            IUnitPositionCanon unitPositionCanon, ICityConfig cityConfig
        ) {
            Grid              = grid;
            UnitFactory       = unitFactory;
            CellScorer        = cellScorer;
            UnitPositionCanon = unitPositionCanon;
            CityConfig        = cityConfig;
        }

        #endregion

        #region instance methods

        #region from IStartingUnitPlacementLogic

        public void PlaceStartingUnitsInRegion(
            MapRegion region, ICivilization owner, IMapTemplate mapTemplate
        ) {
            if(owner.Template.IsBarbaric) {
                return;
            }

            var centralLocation = GetBestStartingCell(region);

            if(centralLocation == null) {
                throw new InvalidOperationException("Failed to find an appropriate central location");
            }

            UnitFactory.BuildUnit(centralLocation, mapTemplate.StartingUnits[0], owner);

            for(int i = 1; i < mapTemplate.StartingUnits.Count; i++) {
                var unitTemplate = mapTemplate.StartingUnits[i];

                var location = Grid.GetCellsInRadius(centralLocation, 2).Where(
                    cell => !UnitPositionCanon.GetPossessionsOfOwner(cell).Any() &&
                            UnitFactory.CanBuildUnit(cell, unitTemplate, owner) &&
                            cell != null
                ).FirstOrDefault();

                if(location == null) {
                    throw new InvalidOperationException(
                        string.Format("Failed to place starting unit {0} for civ {1}", unitTemplate, owner)
                    );
                }else {
                    UnitFactory.BuildUnit(location, unitTemplate, owner);
                }
            }
        }

        #endregion

        private IHexCell GetBestStartingCell(MapRegion region) {
            var validLocations = region.Cells.Where(cell => IsValidForLandUnit(cell)).ToList();

            validLocations.Sort(GetScoreComparisonFunction(region));

            return validLocations.First();
        }

        private bool IsValidForLandUnit(IHexCell cell) {
            return !cell.Terrain.IsWater() && cell.Shape != CellShape.Mountains;
        }

        private Comparison<IHexCell> GetScoreComparisonFunction(MapRegion region) {
            var cellScores = new Dictionary<IHexCell, float>();
            var cellsInRange = new Dictionary<IHexCell, IHexCell[]>();

            foreach(var cell in region.Cells) {
                cellScores[cell] = CellScorer.GetScoreOfCell(cell);

                cellsInRange[cell] = Grid.GetCellsInRadius(cell, CityConfig.MaxBorderRange)
                                         .Where(nearby => region.Cells.Contains(nearby))
                                         .ToArray();
            }

            var cityPlacementScore = new Dictionary<IHexCell, float>();

            return delegate(IHexCell cellOne, IHexCell cellTwo) {
                float cellOneScore, cellTwoScore;

                if(!cityPlacementScore.TryGetValue(cellOne, out cellOneScore)) {
                    cellOneScore = cellsInRange[cellOne].Select(neighbor => cellScores[neighbor]).Sum();

                    cityPlacementScore[cellOne] = cellOneScore;
                }

                if(!cityPlacementScore.TryGetValue(cellTwo, out cellTwoScore)) {
                    cellTwoScore = cellsInRange[cellTwo].Select(neighbor => cellScores[neighbor]).Sum();

                    cityPlacementScore[cellTwo] = cellTwoScore;
                }

                return cellTwoScore.CompareTo(cellOneScore);
            };
        }

        #endregion
        
    }

}
