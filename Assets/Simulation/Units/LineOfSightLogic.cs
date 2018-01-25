using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public class LineOfSightLogic : ILineOfSightLogic {

        #region instance fields and properties

        private IHexGrid Grid;

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public LineOfSightLogic(IHexGrid grid, IUnitPositionCanon unitPositionCanon) {
            Grid = grid;
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from ILineOfSightLogic

        public bool CanUnitSeeCell(IUnit unit, IHexCell cell) {
            var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);

            var lineBetween = Grid.GetCellsInLine(unitPosition, cell);

            if(lineBetween.Count == 1) {
                return true;
            }else if(lineBetween.Count == 2) {
                return CanUnitSeeCell_AdjacentCase(unitPosition, cell);
            }else if(lineBetween.Count <= unit.VisionRange + 1) {
                return CanUnitSeeCell_WithinVisionRangeCase(lineBetween);
            }else if(lineBetween.Count == unit.VisionRange + 2) {
                return CanUnitSeeCell_JustBeyondVisionRangeCase(lineBetween);
            }else {
                return false;
            }
        }

        public IEnumerable<IHexCell> GetCellsVisibleToUnit(IUnit unit) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return Grid.GetCellsInRadius(unitLocation, unit.VisionRange + 1).Where(cell => CanUnitSeeCell(unit, cell));
        }

        #endregion

        private bool CanUnitSeeCell_AdjacentCase(IHexCell location, IHexCell cellToSee) {
            return location.Elevation >= cellToSee.Elevation
                || cellToSee.Elevation - location.Elevation < 3;
        }

        private bool CanUnitSeeCell_WithinVisionRangeCase(List<IHexCell> cellLine){
            IHexCell location = cellLine[0];
            IHexCell cellToSee = cellLine.Last();

            for(int i = 1; i <= cellLine.Count - 2; i++) {
                var intermediate = cellLine[i];

                int intermediateEffectiveElevation = intermediate.Elevation + intermediate.Feature == TerrainFeature.Forest ? 1 : 0;

                if(intermediateEffectiveElevation > location.Elevation) {
                    return false;
                }
            }            

            return location.Elevation >= cellToSee.Elevation
                || cellToSee.Elevation - location.Elevation < 4;
        }

        private bool CanUnitSeeCell_JustBeyondVisionRangeCase(List<IHexCell> cellLine){
            IHexCell location = cellLine[0];
            IHexCell cellToSee = cellLine.Last();

            if(location.Elevation >= cellToSee.Elevation) {
                for(int i = 1; i <= cellLine.Count - 2; i++) {
                    var intermediate = cellLine[i];

                    int intermediateEffectiveElevation = intermediate.Elevation + intermediate.Feature == TerrainFeature.Forest ? 1 : 0;

                    if(!intermediate.IsUnderwater || intermediateEffectiveElevation > location.Elevation) {
                        return false;
                    }
                }
                return true;

            }else {
                for(int i = 1; i <= cellLine.Count - 2; i++) {
                    var intermediate = cellLine[i];

                    int intermediateEffectiveElevation = intermediate.Elevation + intermediate.Feature == TerrainFeature.Forest ? 1 : 0;

                    if(intermediateEffectiveElevation > location.Elevation) {
                        return false;
                    }
                }
                return location.Elevation >= cellToSee.Elevation || (
                    cellToSee.Elevation - location.Elevation > 1 &&
                    cellToSee.Elevation - location.Elevation < 5
                );
            }
        }

        #endregion
        
    }

}
