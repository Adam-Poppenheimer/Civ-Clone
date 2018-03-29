using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class FreshWaterCanon : IFreshWaterCanon {

        #region instance fields and properties

        private IHexGrid Grid;
        private IRiverCanon RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public FreshWaterCanon(IHexGrid grid, IRiverCanon riverCanon) {
            Grid       = grid;
            RiverCanon = riverCanon;
        }

        #endregion

        #region instance methods

        #region from IFreshWaterCanon

        public bool DoesCellHaveAccessToFreshWater(IHexCell cell) {
            return Grid.GetCellsInRadius(cell, 1).Where(nearbyCell => RiverCanon.HasRiver(nearbyCell)).Count() > 0;
        }

        #endregion

        #endregion
        
    }

}
