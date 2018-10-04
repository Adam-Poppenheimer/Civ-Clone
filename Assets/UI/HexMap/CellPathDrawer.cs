using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public class CellPathDrawer : ICellPathDrawer {

        #region instance fields and properties

        private List<IHexCell> LastPath;



        private IHexCellOverlayManager OverlayManager;

        #endregion

        #region constructors

        [Inject]
        public CellPathDrawer(IHexCellOverlayManager overlayManager){
            OverlayManager = overlayManager;
        }

        #endregion

        #region instance methods

        #region from ITilePathDrawer

        public void ClearPath() {
            if(LastPath != null) {
                foreach(var cell in LastPath) {
                    OverlayManager.ClearOverlay(cell);
                }

                LastPath.Clear();
            }
        }

        public void DrawPath(List<IHexCell> path) {
            ClearPath();
            LastPath = new List<IHexCell>(path);

            foreach(var cell in path) {
                OverlayManager.ShowOverlayOfCell(cell, CellOverlayType.PathIndicator);
            }
        }

        #endregion

        #endregion
        
    }

}
