using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using BetterUI;

using Assets.UI.Core;
using Assets.Simulation.Core;

namespace Assets.UI.Core {

    public class EndTurnDisplay : UIPanel {

        #region instance fields and properties

        [SerializeField] private Button EndTurnButton;

        [SerializeField] private string EndTurnKeyName;

        private ITurnExecuter TurnExecuter;

        private PlayerSignals PlayerSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(PlayerSignals playerSignals) {
            PlayerSignals = playerSignals;
        }

        #region Unity message methods

        private void Start() {
            EndTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }

        protected override void DoOnUpdate() {
            if(Input.GetButtonDown("Submit")) {
                PlayerSignals.EndTurnRequestedSignal.OnNext(Unit.Default);
            }
        }

        #endregion

        private void OnEndTurnButtonClicked() {
            PlayerSignals.EndTurnRequestedSignal.OnNext(Unit.Default);
        }

        #endregion

    }

}


