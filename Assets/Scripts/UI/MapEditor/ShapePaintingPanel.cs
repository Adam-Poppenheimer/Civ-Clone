using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class ShapePaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool IsPainting;

        private CellShape ActiveShape;



        private ICellModificationLogic ModLogic;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(ICellModificationLogic modLogic) {
            ModLogic = modLogic;
        }

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(IsPainting && ModLogic.CanChangeShapeOfCell(cell, ActiveShape)) {
                ModLogic.ChangeShapeOfCell(cell, ActiveShape);
            }
        }

        #endregion

        public void SetActiveShape(int index) {
            IsPainting = index >= 0;
            if(IsPainting) {
                ActiveShape = (CellShape)index;
            }
        }

        #endregion

    }

}
