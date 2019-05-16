using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;

namespace Assets.UI {

    public abstract class DisplayBase<T> : MonoBehaviour, IDisplayBase where T : class {

        #region instance fields and properties

        public T ObjectToDisplay { get; set; }     
        
        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();

        #endregion

        #region instance methods

        [Inject]
        private void InjectSignals(CoreSignals coreSignals) {
            SignalSubscriptions.Add(coreSignals.RoundBegan.Subscribe(OnRoundBegan));
        }

        #region Unity messages

        private void OnDestroy() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
        }

        protected virtual void DoOnDestroy() { }

        #endregion

        #region signal responses

        private void OnRoundBegan(int turnCount) {
            if(gameObject.activeInHierarchy) {
                Refresh();
            }
        }

        #endregion

        public virtual void Refresh() { }

        #endregion

    }

}
