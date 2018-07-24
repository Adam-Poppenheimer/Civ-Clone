using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class OceanGenerator : IOceanGenerator {

        #region instance fields and properties

        private ICellModificationLogic ModLogic;
        private IHexGrid               Grid;

        #endregion

        #region constructors

        [Inject]
        public OceanGenerator(
            ICellModificationLogic modLogic, IHexGrid grid
        ) {
            ModLogic = modLogic;
            Grid     = grid;
        }

        #endregion

        #region instance methods

        #region from IOceanGenerator

        public void GenerateOcean(
            MapRegion ocean, IOceanGenerationTemplate template, IEnumerable<IHexCell> continentCells
        ) {
            foreach(var cell in ocean.Cells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.DeepWater);
            }
        }

        #endregion

        #endregion
        
    }

}
