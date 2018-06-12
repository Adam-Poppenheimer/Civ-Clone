using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    public abstract class CellPaintingStateBase : StateMachineBehaviour {

        #region instance fields and properties
        
        protected CellPaintingPanelBase PanelToControl { get; set; }

        private BrushPanel            BrushPanel;
        private UIStateMachineBrain   Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(BrushPanel brushPanel, UIStateMachineBrain brain) {
            BrushPanel = brushPanel;
            Brain      = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();

            Brain.EnableCameraMovement();
            Brain.DisableCellHovering();

            Brain.ListenForTransitions(TransitionType.ReturnViaButton);

            BrushPanel.SubscribePainter(PanelToControl);

            PanelToControl.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();

            BrushPanel.UnsubscribePainter(PanelToControl);

            PanelToControl.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
