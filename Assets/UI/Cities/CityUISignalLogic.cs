using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public class CityUISignalLogic : IDisplaySignalLogic<ICity> {

        #region instance fields and properties

        public IObservable<ICity> OpenDisplayRequested {
            get { return OpenDisplaySubject; }
        }
        private ISubject<ICity> OpenDisplaySubject;

        public IObservable<ICity> CloseDisplayRequested {
            get { return CloseDisplaySubject; }
        }
        private ISubject<ICity> CloseDisplaySubject;

        private GameObject CityDisplayRoot;

        private ICity CurrentlySelectedCity;

        private MonoBehaviour CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public CityUISignalLogic(
            [Inject(Id = "Clicked Anywhere Signal")] IObservable<Unit> clickedAnywhereSignal,
            [Inject(Id = "Cancel Pressed Signal")]   IObservable<Unit> cancelPressedSignal,
            [Inject(Id = "City Clicked Signal")]     IObservable<ICity> cityClickedSignal,
            [Inject(Id = "City Display Root")]       GameObject cityDisplayRoot,
            [Inject(Id = "Coroutine Invoker")]       MonoBehaviour coroutineInvoker
        ){
            OpenDisplaySubject  = new Subject<ICity>();
            CloseDisplaySubject = new Subject<ICity>();

            clickedAnywhereSignal.Subscribe(OnClickedAnywhereFired);
            
            cancelPressedSignal  .Subscribe(OnCancelPressedFired);
            cityClickedSignal    .Subscribe(OnCityClickedFired);

            CityDisplayRoot = cityDisplayRoot;
            CoroutineInvoker = coroutineInvoker;
        }

        #endregion

        #region instance methods

        private void OnClickedAnywhereFired(Unit unit) {
            if(CurrentlySelectedCity != null) {
                var selectedObject = EventSystem.current.currentSelectedGameObject;

                var objectIsDisplayChild = selectedObject != null ? selectedObject.transform.IsChildOf(CityDisplayRoot.transform)       : false;
                var objectIsCityChild    = selectedObject != null ? selectedObject.transform.IsChildOf(CurrentlySelectedCity.transform) : false;

                if(!objectIsDisplayChild && !objectIsCityChild) {
                    CloseDisplaySubject.OnNext(CurrentlySelectedCity);
                    CurrentlySelectedCity = null;
                }
            }
        }

        private void OnCancelPressedFired(Unit unit) {
            if(CurrentlySelectedCity != null) {
                CloseDisplaySubject.OnNext(CurrentlySelectedCity);
                CurrentlySelectedCity = null;
            }
        }

        private void OnCityClickedFired(ICity clickedCity) {
            if(CurrentlySelectedCity != null) {
                CloseDisplaySubject.OnNext(CurrentlySelectedCity);
            }

            OpenDisplaySubject.OnNext(CurrentlySelectedCity);
            CurrentlySelectedCity = clickedCity;

            CoroutineInvoker.StartCoroutine(CauseSelectionChangeCoroutine(clickedCity));
        }

        private IEnumerator CauseSelectionChangeCoroutine(ICity newlySelectedCity) {
            yield return new WaitForEndOfFrame();

            CurrentlySelectedCity = newlySelectedCity;
            OpenDisplaySubject.OnNext(newlySelectedCity);
        }

        #endregion

    }

}
