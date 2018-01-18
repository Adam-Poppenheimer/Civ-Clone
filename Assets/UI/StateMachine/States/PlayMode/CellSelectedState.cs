using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

using Assets.UI.HexMap;
using Assets.UI.Core;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class CellSelectedState : StateMachineBehaviour {

        #region instance fields and properties

        private List<TileDisplayBase> DisplaysToManage;

        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(List<TileDisplayBase> allTileDisplays,
            UIStateMachineBrain brain
        ){
            DisplaysToManage = allTileDisplays;
            Brain            = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastCellClicked;
                display.Refresh();
            }

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.ReturnViaClick,
                TransitionType.ToCitySelected, TransitionType.ToCellSelected);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }
        }

        #endregion

        #endregion

    }

}
