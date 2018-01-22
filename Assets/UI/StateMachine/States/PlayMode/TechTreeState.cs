﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Core;
using Assets.UI.Technology;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class TechTreeState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private TechTreeDisplay TechTreeDisplay;

        private GameCore GameCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, TechTreeDisplay techTreeDisplay, GameCore gameCore
        ){
            Brain           = brain;
            TechTreeDisplay = techTreeDisplay;
            GameCore        = gameCore;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);

            TechTreeDisplay.ObjectToDisplay = GameCore.PlayerCivilization;
            TechTreeDisplay.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TechTreeDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
