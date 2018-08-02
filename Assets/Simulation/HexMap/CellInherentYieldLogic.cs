using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class InherentCellYieldLogic : IInherentCellYieldLogic {

        #region instance fields and properties

        private IHexMapConfig Config;

        #endregion

        #region constructors

        [Inject]
        public InherentCellYieldLogic(IHexMapConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ICellInherentYieldLogic

        public YieldSummary GetInherentCellYield(IHexCell cell) {
            YieldSummary retval = YieldSummary.Empty;

            if(cell.Vegetation != CellVegetation.None) {
                retval = Config.GetYieldOfVegetation(cell.Vegetation);
            }else if(cell.Shape != CellShape.Flatlands) {
                retval = Config.GetYieldOfShape(cell.Shape);
            }else {
                retval = Config.GetYieldOfTerrain(cell.Terrain);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
