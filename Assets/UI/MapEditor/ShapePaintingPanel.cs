using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class ShapePaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool IsPainting;

        private TerrainShape ActiveShape;

        #endregion

        #region instance methods

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(IsPainting) {
                cell.Shape = ActiveShape;

                if(cell.IsUnderwater && ActiveShape != TerrainShape.Flatlands) {
                    cell.Terrain = TerrainType.Grassland;
                }
            }
        }

        #endregion

        public void SetActiveShape(int index) {
            IsPainting = index >= 0;
            if(IsPainting) {
                ActiveShape = (TerrainShape)index;
            }
        }

        #endregion

    }

}
