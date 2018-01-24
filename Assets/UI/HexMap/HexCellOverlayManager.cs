using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public class HexCellOverlayManager {

        #region instance fields and properties

        private List<IHexCellOverlay> ShownOverlays = new List<IHexCellOverlay>();

        #endregion

        #region instance methods

        public void ShowOverlayOfCell(IHexCell cell, CellOverlayType type) {
            cell.Overlay.SetDisplayType(type);
            cell.Overlay.Show();

            ShownOverlays.Add(cell.Overlay);
        }

        public void ClearOverlay(IHexCell cell) {
            cell.Overlay.Clear();
            cell.Overlay.Hide();

            ShownOverlays.Remove(cell.Overlay);
        }

        public void ClearAllOverlays() {
            foreach(var overlay in ShownOverlays) {
                overlay.Clear();
                overlay.Hide();
            }

            ShownOverlays.Clear();
        }

        #endregion

    }

}
