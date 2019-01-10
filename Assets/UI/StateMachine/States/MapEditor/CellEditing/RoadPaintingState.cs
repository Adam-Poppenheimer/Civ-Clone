using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    public class RoadPaintingState : CellPaintingStateBase {

        #region instance methods

        [Inject]
        public void InjectPanel(
            [Inject(Id = "Road Painting Panel")] CellPaintingPanelBase roadPaintingPanel
        ) {
            PanelToControl = roadPaintingPanel;
        }

        #endregion

    }

}
