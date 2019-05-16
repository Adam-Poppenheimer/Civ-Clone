using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    public class VegetationPaintingState : CellPaintingStateBase {

        #region instance methods

        [Inject]
        public void InjectPanel(
            [Inject(Id = "Vegetation Painting Panel")] CellPaintingPanelBase vegetationPaintingPanel
        ) {
            PanelToControl = vegetationPaintingPanel;
        }

        #endregion

    }

}
