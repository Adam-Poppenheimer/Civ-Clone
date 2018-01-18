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
            return true;
        }

        public IEnumerable<IHexCell> GetCellsVisibleToUnit(IUnit unit) {
            return Grid.GetCellsInRadius(UnitPositionCanon.GetOwnerOfPossession(unit), 2);
        }

        #endregion

        #endregion
        
    }

}
