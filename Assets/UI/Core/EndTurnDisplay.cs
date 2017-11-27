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

        private ITurnExecuter TurnExecuter;

        private IRecordkeepingCityFactory CityFactory;

        private TurnBeganSignal TurnBeganSignal;
        private TurnEndedSignal TurnEndedSignal;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITurnExecuter turnExecuter, IRecordkeepingCityFactory cityFactory,
            TurnBeganSignal turnBeganSignal, TurnEndedSignal turnEndedSignal) {

            TurnExecuter = turnExecuter;
            CityFactory = cityFactory;

            TurnBeganSignal = turnBeganSignal;
            TurnEndedSignal = turnEndedSignal;
        }

        #region Unity message methods

        private void Start() {
            EndTurnButton.onClick.AddListener(EndTurn);
        }

        protected override void DoOnUpdate() {
            if(Input.GetButtonDown("Submit")) {
                EndTurn();
            }
        }

        #endregion

        private void EndTurn() {
            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.EndTurnOnCity(city);
            }

            TurnEndedSignal.Fire(0);

            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.BeginTurnOnCity(city);
            }

            TurnBeganSignal.Fire(0);
        }

        #endregion

    }

}


