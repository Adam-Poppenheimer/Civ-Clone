using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Core;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public abstract class CityDisplayBase : MonoBehaviour {

        #region instance fields and properties

        protected ICity CityToDisplay { get; set; }

        private IObservable<Unit> DeselectedSignal;
        private IDisposable DeselectSubscription;

        #endregion

        #region instance methods

        [Inject]
        private void InjectSignals(
            CityClickedSignal clickedSignal,
            [Inject(Id = "CityDisplay Deselected")] IObservable<Unit> deslectedSignal,
            TurnBeganSignal turnBeganSignal
        ){
            clickedSignal.AsObservable.Subscribe(OnCityClicked);
            
            DeselectedSignal = deslectedSignal;
        }

        #region Unity message methods

        private void OnEnable() {
            if(CityToDisplay != null) {
                DisplayCity(CityToDisplay);
            }
            DeselectSubscription = DeselectedSignal.Subscribe(OnDeselected);          
        }

        private void OnDisable() {
            DeselectSubscription.Dispose();
            DeselectSubscription = null;
        }

        #endregion

        #region signal responses

        private void OnCityClicked(Tuple<ICity, PointerEventData> dataTuple) {
            CityToDisplay = dataTuple.Item1;
            gameObject.SetActive(true);
        }

        private void OnDeselected(Unit unit) {
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
