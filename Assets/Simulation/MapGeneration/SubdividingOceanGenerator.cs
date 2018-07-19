using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class SubdividingOceanGenerator : IOceanGenerator {

        #region instance fields and properties

        private ICellModificationLogic ModLogic;
        private IHexGrid               Grid;

        #endregion

        #region constructors

        [Inject]
        public SubdividingOceanGenerator(
            ICellModificationLogic modLogic, IHexGrid grid
        ) {
            ModLogic = modLogic;
            Grid     = grid;
        }

        #endregion

        #region instance methods

        #region from IOceanGenerator

        public void GenerateOcean(MapRegion ocean) {
            foreach(var cell in ocean.Cells) {
                ModLogic.CanChangeTerrainOfCell(cell, CellTerrain.DeepWater);
            }
        }

        #endregion

        #endregion
        
    }

}
