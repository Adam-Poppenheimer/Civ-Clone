using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    public class FeaturePaintingState : CellPaintingStateBase {

        #region instance methods

        [Inject]
        public void InjectPanel(
            [Inject(Id = "Feature Painting Panel")] CellPaintingPanelBase featurePaintingPanel) {
            PanelToControl = featurePaintingPanel;
        }

        #endregion

    }

}
