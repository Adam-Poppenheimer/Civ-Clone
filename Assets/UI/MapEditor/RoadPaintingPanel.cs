using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class RoadPaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool AddingOrRemoving;

        #endregion

        #region instance methods

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(AddingOrRemoving && !cell.IsUnderwater) {
                cell.HasRoads = true;
            }else {
                cell.HasRoads = false;
            }
        }

        #endregion

        public void SetRoadMode(bool addingOrRemoving) {
            AddingOrRemoving = addingOrRemoving;
        }

        #endregion
        
    }

}
