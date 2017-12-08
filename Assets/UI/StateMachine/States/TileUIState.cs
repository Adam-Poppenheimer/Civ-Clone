using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.GameMap;

using Assets.UI.GameMap;

namespace Assets.UI.StateMachine.States {

    public class TileUIState : StateMachineBehaviour {

        #region instance fields and properties

        public IMapTile TileToDisplay { get; set; }

        private List<TileDisplayBase> DisplaysToManage;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(List<TileDisplayBase> allTileDisplays) {
            DisplaysToManage = allTileDisplays;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = TileToDisplay;
                display.Refresh();
            }
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
