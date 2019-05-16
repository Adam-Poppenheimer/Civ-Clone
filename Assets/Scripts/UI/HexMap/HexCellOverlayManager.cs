using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public class HexCellOverlayManager : IHexCellOverlayManager {

        #region instance fields and properties

        private Dictionary<IHexCell, HexCellOverlay> OverlayOfCell = 
            new Dictionary<IHexCell, HexCellOverlay>();



        private HexCellOverlay.Pool OverlayPool;

        #endregion

        #region constructors

        [Inject]
        public HexCellOverlayManager(HexCellOverlay.Pool overlayPool) {
            OverlayPool = overlayPool;
        }

        #endregion

        #region instance methods

        public void ShowOverlayOfCell(IHexCell cell, CellOverlayType type) {
            HexCellOverlay overlayToShow;

            if(!OverlayOfCell.TryGetValue(cell, out overlayToShow)) {
                overlayToShow = OverlayPool.Spawn();

                OverlayOfCell[cell] = overlayToShow;
            }

            overlayToShow.SetDisplayType(type);
            overlayToShow.CellToDisplay = cell;
            overlayToShow.Show();
        }

        public void ClearOverlay(IHexCell cell) {
            HexCellOverlay overlayToClear;

            if(OverlayOfCell.TryGetValue(cell, out overlayToClear)) {
                OverlayOfCell.Remove(cell);

                OverlayPool.Despawn(overlayToClear);
            }
        }

        public void ClearAllOverlays() {
            foreach(var cellWithOverlay in OverlayOfCell.Keys.ToArray()) {
                ClearOverlay(cellWithOverlay);
            }
        }

        #endregion

    }

}
