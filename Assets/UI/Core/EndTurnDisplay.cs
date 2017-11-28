using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using BetterUI;

using Assets.Simulation.Cities;
using Assets.Simulation.Core;

namespace Assets.UI.Core {

    public class EndTurnDisplay : UIPanel {

        #region instance fields and properties

        [SerializeField] private Button EndTurnButton;

        [SerializeField] private string EndTurnKeyName;

        private ITurnExecuter TurnExecuter;

        private EndTurnRequestedSignal EndTurnRequestedSignal;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(EndTurnRequestedSignal endTurnRequestedSignal) {
            EndTurnRequestedSignal = endTurnRequestedSignal;
        }

        #region Unity message methods

        private void Start() {
            EndTurnButton.onClick.AddListener(() => EndTurnRequestedSignal.Fire());
        }

        protected override void DoOnUpdate() {
            if(Input.GetButtonDown("Submit")) {
                EndTurnRequestedSignal.Fire();
            }
        }

        #endregion

        #endregion

    }

}


