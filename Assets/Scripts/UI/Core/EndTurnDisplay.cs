using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using BetterUI;

using Assets.Simulation.Players;
using Assets.Simulation.Core;

namespace Assets.UI.Core {

    public class EndTurnDisplay : UIPanel {

        #region instance fields and properties

        [SerializeField] private Button EndTurnButton = null;





        private IGameCore     GameCore;
        private PlayerSignals PlayerSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IGameCore gameCore, PlayerSignals playerSignals) {
            GameCore      = gameCore;
            PlayerSignals = playerSignals;
        }

        #region Unity message methods

        private void Start() {
            EndTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }

        protected override void DoOnUpdate() {
            if(Input.GetButtonDown("Submit")) {
                PlayerSignals.EndTurnRequested.OnNext(GameCore.ActivePlayer);
            }
        }

        #endregion

        private void OnEndTurnButtonClicked() {
            PlayerSignals.EndTurnRequested.OnNext(GameCore.ActivePlayer);
        }

        #endregion

    }

}


