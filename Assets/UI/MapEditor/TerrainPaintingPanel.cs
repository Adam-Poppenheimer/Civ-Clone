using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class TerrainPaintingPanel : CellPaintingPanelBase {

        #region instance fields and properties

        private bool IsPaintingTerrain;

        private CellTerrain ActiveTerrain;



        private ICellModificationLogic CellModificationLogic;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(ICellModificationLogic cellModificationLogic) {
            CellModificationLogic = cellModificationLogic;
        }

        #region from CellPaintingPanelBase

        protected override void EditCell(IHexCell cell) {
            if(IsPaintingTerrain && CellModificationLogic.CanChangeTerrainOfCell(cell, ActiveTerrain)) {
                CellModificationLogic.ChangeTerrainOfCell(cell, ActiveTerrain);
            }
        }

        #endregion

        public void SetActiveTerrain(int index) {
            IsPaintingTerrain = index >= 0;
            if(IsPaintingTerrain) {
                ActiveTerrain = (CellTerrain)index;
            }
        }

        #endregion

    }

}
