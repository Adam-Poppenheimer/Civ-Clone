using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Core;

namespace Assets.UI {

    public abstract class DisplayBase<T> : MonoBehaviour where T : class {

        #region instance fields and properties

        public T ObjectToDisplay { get; set; }

        private IDisposable SelectRequestedSubscription;

        private IObservable<T> DeselectRequestedSignal;
        private IDisposable DeselectRequestedSubscription;        

        #endregion

        #region instance methods

        [Inject]
        private void InjectSignals(
            [Inject(Id = "Select Requested Signal"  )] IObservable<T> selectRequestedSignal,
            [Inject(Id = "Deselect Requested Signal")] IObservable<T> deselectRequestedSignal,
            TurnBeganSignal turnBeganSignal
        ) {
            SelectRequestedSubscription = selectRequestedSignal.Subscribe(OnSelectRequested);

            DeselectRequestedSignal = deselectRequestedSignal;

            turnBeganSignal.Listen(OnTurnBegan);
        }

        #region Unity message methods

        private void OnDestroy() {
            if(SelectRequestedSubscription != null) {
                SelectRequestedSubscription.Dispose();
            }
            
            if(DeselectRequestedSubscription != null) {
                DeselectRequestedSubscription.Dispose();
            }
            

            DoOnDestroy();
        }

        protected virtual void DoOnDestroy() { }

        #endregion

        #region signal responses

        private void OnSelectRequested(T objectToSelect) {
            ObjectToDisplay = objectToSelect;
            gameObject.SetActive(true);

            DeselectRequestedSubscription = DeselectRequestedSignal.Subscribe(OnDeselectRequested);

            Refresh();
        }
        
        private void OnDeselectRequested(T deselectedObject) {
            ObjectToDisplay = null;
            gameObject.SetActive(false);

            DeselectRequestedSubscription.Dispose();
            DeselectRequestedSubscription = null;
        }

        private void OnTurnBegan(int turnCount) {
            if(gameObject.activeInHierarchy) {
                Refresh();
            }
        }

        #endregion

        public virtual void Refresh() { }

        #endregion

    }

}
