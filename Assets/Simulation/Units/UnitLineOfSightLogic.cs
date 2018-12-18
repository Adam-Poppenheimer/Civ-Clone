using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public class UnitLineOfSightLogic : IUnitLineOfSightLogic {

        #region instance fields and properties

        private IHexGrid           Grid;
        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitLineOfSightLogic(IHexGrid grid, IUnitPositionCanon unitPositionCanon) {
            Grid              = grid;
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitLineOfSightLogic

        public IEnumerable<IHexCell> GetCellsVisibleToUnit(IUnit unit) {
            var retval = new List<IHexCell>();

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            foreach(var cell in Grid.GetCellsInRadius(unitLocation, unit.VisionRange)) {
                if(!HasObstructionsBetween(unitLocation, cell)) {
                    retval.Add(cell);
                }
            }

            foreach(var cell in Grid.GetCellsInRing(unitLocation, unit.VisionRange + 1)) {
                if(!HasObstructionsBetween(unitLocation, cell) && cell.Terrain.IsWater()) {
                    retval.Add(cell);
                }
            }

            return retval;
        }

        #endregion

        private bool HasObstructionsBetween(IHexCell fromCell, IHexCell toCell) {
            if(fromCell != toCell && !Grid.GetNeighbors(fromCell).Contains(toCell)) {

                var cellLine = Grid.GetCellsInLine(fromCell, toCell).Where(cell => cell != fromCell);

                if(fromCell.Shape == CellShape.Flatlands) {
                    return cellLine.Any(cell => cell.Shape != CellShape.Flatlands || cell.Vegetation.HasTrees());

                }else if(fromCell.Shape == CellShape.Hills || fromCell.Shape == CellShape.Mountains) {
                    return cellLine.Any(
                        cell => cell.Shape == CellShape.Mountains ||
                        (cell.Shape == CellShape.Hills && cell.Vegetation.HasTrees())
                    );
                }
            }
            
            return false;
        }

        #endregion
        
    }

}
