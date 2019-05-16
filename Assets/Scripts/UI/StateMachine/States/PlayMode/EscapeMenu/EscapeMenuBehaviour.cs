using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Core;

namespace Assets.UI.StateMachine.States.PlayMode.EscapeMenu {

    public class EscapeMenuBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain             Brain;
        private RectTransform                   EscapeMenuContainer;
        private List<IPlayModeSensitiveElement> PlayModeSensitiveElements;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "Escape Menu Container")] RectTransform escapeMenuContainer,
            IFreeGreatPeopleCanon freeGreatPeopleCanon,
            List<IPlayModeSensitiveElement> playModeSensitiveElements
        ) {
            Brain                     = brain;
            EscapeMenuContainer       = escapeMenuContainer;
            PlayModeSensitiveElements = playModeSensitiveElements;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            EscapeMenuContainer.gameObject.SetActive(true);

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = false;
            }

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            EscapeMenuContainer.gameObject.SetActive(false);

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = true;
            }

            Brain.EnableCameraMovement();
        }

        #endregion

        #endregion

    }

}
