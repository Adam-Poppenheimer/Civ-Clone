using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.StateMachine.States.PlayMode.EscapeMenu {

    public class EscapeMenuBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain     Brain;
        private IFreeBuildingsCanon     FreeBuildingsCanon;
        private RectTransform           EscapeMenuContainer;
        private IFreeUnitsResponder     FreeUnitsResponder;
        private IFreeBuildingsResponder FreeBuildingsResponder;
        private IFreeGreatPeopleCanon   FreeGreatPeopleCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IFreeBuildingsCanon freeBuildingsCanon,
            [Inject(Id = "Escape Menu Container")] RectTransform escapeMenuContainer,
            IFreeUnitsResponder freeUnitsResponder, IFreeBuildingsResponder freeBuildingsResponder,
            IFreeGreatPeopleCanon freeGreatPeopleCanon
        ) {
            Brain                  = brain;
            FreeBuildingsCanon     = freeBuildingsCanon;
            EscapeMenuContainer    = escapeMenuContainer;
            FreeUnitsResponder     = freeUnitsResponder;
            FreeBuildingsResponder = freeBuildingsResponder;
            FreeGreatPeopleCanon   = freeGreatPeopleCanon;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            EscapeMenuContainer.gameObject.SetActive(true);

            FreeBuildingsCanon    .ApplyBuildingsToCities = false;
            FreeUnitsResponder    .IsActive               = false;
            FreeBuildingsResponder.IsActive               = false;
            FreeGreatPeopleCanon  .IsActive               = false;

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            EscapeMenuContainer.gameObject.SetActive(false);

            FreeBuildingsCanon    .ApplyBuildingsToCities = true;
            FreeUnitsResponder    .IsActive               = true;
            FreeBuildingsResponder.IsActive               = true;
            FreeGreatPeopleCanon  .IsActive               = true;

            Brain.EnableCameraMovement();
        }

        #endregion

        #endregion

    }

}
