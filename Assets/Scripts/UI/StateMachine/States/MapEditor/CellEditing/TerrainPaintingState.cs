using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    public class TerrainPaintingState : CellPaintingStateBase {

        #region instance methods

        [Inject]
        public void InjectPanel(
            [Inject(Id = "Terrain Painting Panel")] CellPaintingPanelBase terrainPaintingPanel
        ) {
            PanelToControl = terrainPaintingPanel;
        }

        #endregion

    }

}
