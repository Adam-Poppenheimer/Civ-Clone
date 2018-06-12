using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class FeaturePaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool IsPainting;

        private TerrainFeature ActiveFeature;

        #endregion

        #region instance methods

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(IsPainting && !cell.IsUnderwater) {
                cell.Feature = ActiveFeature;
            }
        }

        #endregion

        public void SetActiveFeature(int index) {
            IsPainting = index >= 0;
            if(IsPainting) {
                ActiveFeature = (TerrainFeature)index;
            }
        }

        #endregion
        
    }

}
