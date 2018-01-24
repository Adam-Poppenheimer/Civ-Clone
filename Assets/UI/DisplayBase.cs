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

    public abstract class DisplayBase<T> : MonoBehaviour, IDisplayBase where T : class {

        #region instance fields and properties

        public T ObjectToDisplay { get; set; }     
        
        
        
        private TurnBeganSignal TurnBeginSignal; 

        #endregion

        #region instance methods

        [Inject]
        private void InjectSignals(TurnBeganSignal turnBeganSignal) {
            TurnBeginSignal = turnBeganSignal;
            turnBeganSignal.Listen(OnTurnBegan);
        }

        #region Unity messages

        private void OnDestroy() {
            TurnBeginSignal.Unlisten(OnTurnBegan);
        }

        #endregion

        #region signal responses

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
