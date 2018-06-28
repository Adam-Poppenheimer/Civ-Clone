using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class VegetationPaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool IsPainting;

        private CellVegetation ActiveVegetation;



        private ICellModificationLogic CellModificationLogic;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(ICellModificationLogic cellModificationLogic) {
            CellModificationLogic = cellModificationLogic;
        }

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(IsPainting && CellModificationLogic.CanChangeVegetationOfCell(cell, ActiveVegetation)) {
                CellModificationLogic.ChangeVegetationOfCell(cell, ActiveVegetation);
            }
        }

        #endregion

        public void SetActiveVegetation(int index) {
            IsPainting = index >= 0;
            if(IsPainting) {
                ActiveVegetation = (CellVegetation)index;
            }
        }

        #endregion
        
    }

}
