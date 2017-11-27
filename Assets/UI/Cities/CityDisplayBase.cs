using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UniRx;

using Assets.Simulation.Core;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public abstract class CityDisplayBase : MonoBehaviour {

        #region instance fields and properties

        protected ICity CityToDisplay { get; set; }

        #endregion

        #region instance methods

        [Inject]
        private void InjectSignals(CityClickedSignal clickedSignal, CityPanelCloseRequestSignal closeRequestSignal,
            TurnBeganSignal turnBeganSignal) {

            clickedSignal.AsObservable.Subscribe(OnCityClicked);
            closeRequestSignal.AsObservable.Subscribe(OnCloseRequested);
            turnBeganSignal.AsObservable.Subscribe(OnTurnBegan);
        }

        #region Unity message methods

        private void OnEnable() {
            if(CityToDisplay != null) {
                DisplayCity(CityToDisplay);
            }            
        }

        #endregion

        #region signal responses

        private void OnCityClicked(ICity city) {
            CityToDisplay = city;
            gameObject.SetActive(true);
        }

        private void OnCloseRequested(Unit unit) {
            gameObject.SetActive(false);
        }

        private void OnTurnBegan(int turnCount) {
            if(CityToDisplay != null && gameObject.activeInHierarchy) {
                DisplayCity(CityToDisplay);
            }
        }

        #endregion

        protected virtual void DisplayCity(ICity city) { }

        #endregion

    }

}
