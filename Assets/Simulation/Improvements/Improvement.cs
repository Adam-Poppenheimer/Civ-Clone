using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Improvements {

    public class Improvement : MonoBehaviour, IImprovement {

        #region instance fields and properties

        #region from IImprovement

        public IImprovementTemplate Template { get; set; }

        public bool IsConstructed {
            get {
                var currentStateInfo = StateMachine.GetCurrentAnimatorStateInfo(0);
                return currentStateInfo.IsName("Constructed");
            }
        }

        public bool IsPillaged {
            get {
                var currentStateInfo = StateMachine.GetCurrentAnimatorStateInfo(0);
                return currentStateInfo.IsName("Pillaged");
            }
        }

        public bool IsReadyToConstruct {
            get {
                return !IsConstructed && WorkInvested >= Template.TurnsToConstruct;
            }
        }

        public int WorkInvested { get; set; }

        #endregion

        [SerializeField] public Animator StateMachine;

        private ImprovementSignals Signals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ImprovementSignals signals) {
            Signals = signals;
        }

        #region Unity messages

        private void OnDestroy() {
            Signals.ImprovementBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #region from IImprovement

        public void Construct() {
            if(!IsConstructed) {
                StateMachine.SetTrigger("Constructed Requested");
                Signals.ImprovementConstructedSignal.OnNext(this);
            }
        }

        public void Pillage() {
            if(IsConstructed) {
                StateMachine.SetTrigger("Pillaged Requested");
                Signals.ImprovementPillagedSignal.OnNext(this);
            }else if(!IsPillaged) {
                Destroy(gameObject);
            }
        }

        public void Destroy() {
            if(Application.isPlaying) {
                Destroy(gameObject);
            }else {
                DestroyImmediate(gameObject);
            }
        }

        #endregion

        #endregion

    }

}
