﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class InherentCellYieldLogic : IInherentCellYieldLogic {

        #region instance fields and properties

        private IHexMapSimulationConfig Config;

        #endregion

        #region constructors

        [Inject]
        public InherentCellYieldLogic(IHexMapSimulationConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ICellInherentYieldLogic

        public YieldSummary GetInherentCellYield(IHexCell cell, bool withVegetationCleared) {
            if(cell.Feature != CellFeature.None && Config.DoesFeatureOverrideYield(cell.Feature)) {
                return Config.GetYieldOfFeature(cell.Feature);

            }else if(cell.Vegetation != CellVegetation.None && !withVegetationCleared) {
                return Config.GetYieldOfVegetation(cell.Vegetation);

            }else if(cell.Shape != CellShape.Flatlands) {
                return Config.GetYieldOfShape(cell.Shape);

            }else {
                return Config.GetYieldOfTerrain(cell.Terrain);
            }
        }

        #endregion

        #endregion
        
    }

}
