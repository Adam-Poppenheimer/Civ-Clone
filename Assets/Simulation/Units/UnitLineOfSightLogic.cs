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

            retval.Add(unitLocation);

            var maxPossibleVisionDistance = unit.VisionRange + unitLocation.ViewElevation;

            foreach(var cell in Grid.GetCellsInRadius(unitLocation, maxPossibleVisionDistance)) {
                if(!HasObstructionsBetween(unitLocation, cell)) {
                    retval.Add(cell);
                }
            }

            return retval;
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
