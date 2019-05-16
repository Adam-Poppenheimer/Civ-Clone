using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class FeaturePaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool IsPainting;

        private CellFeature ActiveFeature;



        private ICellModificationLogic ModLogic;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(ICellModificationLogic modLogic) {
            ModLogic = modLogic;
        }

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(IsPainting && ModLogic.CanChangeFeatureOfCell(cell, ActiveFeature)) {
                ModLogic.ChangeFeatureOfCell(cell, ActiveFeature);
            }
        }

        #endregion

        public void SetActiveShape(int index) {
            IsPainting = index >= 0;
            if(IsPainting) {
                ActiveFeature = (CellFeature)index;
            }
        }

        #endregion
        
    }

}
