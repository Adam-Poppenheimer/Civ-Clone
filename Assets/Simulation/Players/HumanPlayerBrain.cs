using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

namespace Assets.Simulation.Players {

    public class HumanPlayerBrain : IPlayerBrain {

        #region instance fields and properties

        #region from IPlayerBrain

        public string Name {
            get { return "Human Brain"; }
        }

        #endregion

        private Coroutine   ExecuteTurnCoroutine;
        private IDisposable EndTurnSubscription;




        private PlayerSignals PlayerSignals;
        private MonoBehaviour CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public HumanPlayerBrain(
            PlayerSignals playerSignals,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ) {
            PlayerSignals    = playerSignals;
            CoroutineInvoker = coroutineInvoker;
        }

        #endregion

        #region instance methods

        #region from IPlayerBrain

        public void ExecuteTurn(Action controlRelinquisher) {
            ExecuteTurnCoroutine = CoroutineInvoker.StartCoroutine(SubscribeToTurnEnding(controlRelinquisher));
        }

        public void RefreshAnalysis() {
            
        }

        public void Clear() {
            if(ExecuteTurnCoroutine != null) {
                CoroutineInvoker.StopCoroutine(ExecuteTurnCoroutine);
            }
            
            if(EndTurnSubscription != null) {
                EndTurnSubscription.Dispose();
            }
        }

        #endregion

        private IEnumerator SubscribeToTurnEnding(Action controlRelinquisher) {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            EndTurnSubscription = PlayerSignals.EndTurnRequested.Subscribe(EndTurn(controlRelinquisher));
        }

        private Action<IPlayer> EndTurn(Action controlRelinquisher) {
            return delegate(IPlayer player) {
                EndTurnSubscription.Dispose();
                controlRelinquisher();
            };
        }

        #endregion
        
    }

}
