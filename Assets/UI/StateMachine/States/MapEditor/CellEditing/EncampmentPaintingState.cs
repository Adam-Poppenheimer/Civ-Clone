using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    public class EncampmentPaintingState : CellPaintingStateBase {

        #region instance methods

        [Inject]
        public void InjectPanel(
            [Inject(Id = "Encampment Painting Panel")] CellPaintingPanelBase encampmentPaintingPanel
        ) {
            PanelToControl = encampmentPaintingPanel;
        }

        #endregion

    }

}
