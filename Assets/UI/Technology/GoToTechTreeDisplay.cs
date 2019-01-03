using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Core;

namespace Assets.UI.Technology {

    public class GoToTechTreeDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private TechSelectionMode SelectionMode;





        private TechTreeDisplay TechTreeDisplay;
        private IGameCore       GameCore;
        private Animator        UIAnimator;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            TechTreeDisplay techTreeDisplay, IGameCore gameCore,
            [Inject(Id = "UI Animator")] Animator uiAnimator
        ) {
            TechTreeDisplay = techTreeDisplay;
            GameCore        = gameCore;
            UIAnimator      = uiAnimator;
        }

        public void OpenTechUI() {
            TechTreeDisplay.SelectionMode   = SelectionMode;
            TechTreeDisplay.ObjectToDisplay = GameCore.ActiveCiv;

            UIAnimator.SetTrigger("Tech Tree Requested");
        }

        #endregion

    }

}
