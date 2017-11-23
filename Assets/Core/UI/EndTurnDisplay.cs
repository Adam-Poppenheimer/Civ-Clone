using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using BetterUI;

using Assets.Cities;

namespace Assets.Core.UI {

    public class EndTurnDisplay : UIPanel {

        #region instance fields and properties

        [SerializeField] private Button EndTurnButton;

        private ITurnExecuter TurnExecuter;

        private IRecordkeepingCityFactory CityFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITurnExecuter turnExecuter, IRecordkeepingCityFactory cityFactory) {
            TurnExecuter = turnExecuter;
            CityFactory = cityFactory;
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

            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.BeginTurnOnCity(city);
            }
        }

        #endregion

    }

}


