using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Units;

namespace Assets.Simulation {

    public class LineOfSightLogic : ILineOfSightLogic {

        #region instance fields and properties

        private IHexGrid                                 Grid;
        private IUnitPositionCanon                       UnitPositionCanon;
        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public LineOfSightLogic(
            IHexGrid grid, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon
        ){
            Grid                = grid;
            UnitPositionCanon   = unitPositionCanon;
            CellPossessionCanon = cellPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ILineOfSightLogic

        public IEnumerable<IHexCell> GetCellsVisibleToUnit(IUnit unit) {
            var retval = new List<IHexCell>();

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            retval.Add(unitLocation);

            var maxPossibleVisionDistance = unit.VisionRange + unitLocation.ViewElevation;

            foreach(var cell in Grid.GetCellsInRadius(unitLocation, maxPossibleVisionDistance)) {
                if(!HasObstructionsBetween(unitLocation, cell)) {
                    retval.Add(cell);
                }
            }

            return retval;
        }

        public IEnumerable<IHexCell> GetCellsVisibleToCity(ICity city) {
            var retval = new HashSet<IHexCell>();

            foreach(var cellInCityBorders in CellPossessionCanon.GetPossessionsOfOwner(city)) {
                retval.Add(cellInCityBorders);
                foreach(var neighbor in Grid.GetNeighbors(cellInCityBorders)) {
                    retval.Add(neighbor);
                }
            }

            return retval;
        }

        public bool CanUnitSeeCell(IUnit unit, IHexCell cell) {
            return GetCellsVisibleToUnit(unit).Contains(cell);
        }

        #endregion

        private bool HasObstructionsBetween(IHexCell fromCell, IHexCell toCell) {
            if(fromCell != toCell && !Grid.GetNeighbors(fromCell).Contains(toCell)) {

                var cellLine = Grid.GetCellsInLine(fromCell, toCell).Where(cell => cell != fromCell);

                foreach(var intermediateCell in cellLine) {
                    int blockingHeight = intermediateCell.ViewElevation;

                    if(intermediateCell.Feature == TerrainFeature.Forest || intermediateCell.Feature == TerrainFeature.Jungle) {
                        blockingHeight++;
                    }

                    if(blockingHeight > fromCell.ViewElevation) {
                        return true;
                    }
                }
            }
            
            return false;
        }

        #endregion

    }

}
