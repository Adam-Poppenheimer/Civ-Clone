using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public class TilePathDrawer : ITilePathDrawer {

        #region instance fields and properties

        private List<IHexCell> LastPath;

        #endregion

        #region constructors

        [Inject]
        public TilePathDrawer(){

        }

        #endregion

        #region instance methods

        #region from ITilePathDrawer

        public void ClearPath() {
            if(LastPath != null) {
                foreach(var cell in LastPath) {
                    cell.Overlay.Clear();
                    cell.Overlay.Hide();
                }

                LastPath.Clear();
            }
        }

        public void DrawPath(List<IHexCell> path) {
            ClearPath();
            LastPath = new List<IHexCell>(path);

            foreach(var cell in path) {
                cell.Overlay.SetDisplayType(CellOverlayType.PathIndicator);
                cell.Overlay.Show();
            }
        }

        #endregion

        #endregion
        
    }

}
