using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    class ShapePaintingState : CellPaintingStateBase {

        #region instance methods

        [Inject]
        public void InjectPanel(
            [Inject(Id = "Shape Painting Panel")] CellPaintingPanelBase shapePaintingPanel
        ) {
            PanelToControl = shapePaintingPanel;
        }

        #endregion

    }

}
