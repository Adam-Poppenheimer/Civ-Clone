using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class TerrainPaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool IsPaintingTerrain;

        private TerrainType ActiveTerrain;

        #endregion

        #region instance methods

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(IsPaintingTerrain) {
                cell.Terrain = ActiveTerrain;

                if(cell.IsUnderwater) {
                    cell.Shape   = TerrainShape.Flatlands;
                    cell.Feature = TerrainFeature.None;
                }
            }
        }

        #endregion

        public void SetActiveTerrain(int index) {
            IsPaintingTerrain = index >= 0;
            if(IsPaintingTerrain) {
                ActiveTerrain = (TerrainType)index;
            }
        }

        #endregion

    }

}
