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

        public bool IsConstructed { get; private set; }

        public bool IsPillaged { get; private set; }

        public bool IsReadyToConstruct {
            get {
                return !IsConstructed && WorkInvested >= Template.TurnsToConstruct;
            }
        }

        public float WorkInvested { get; set; }

        #endregion

        private ImprovementSignals Signals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ImprovementSignals signals) {
            Signals = signals;
        }

        #region Unity messages

        private void OnDestroy() {
            Signals.BeingDestroyed.OnNext(this);
        }

        #endregion

        #region from IImprovement

        public void Construct() {
            if(!IsConstructed) {
                IsPillaged    = false;
                IsConstructed = true;
                WorkInvested  = 0f;

                Signals.Constructed.OnNext(this);
            }
        }

        public void Pillage() {
            if(IsConstructed) {
                IsConstructed = false;
                IsPillaged    = true;         
                WorkInvested  = 0f;
                       
                Signals.Pillaged.OnNext(this);

            }else if(!IsPillaged) {
                Destroy(gameObject);
            }
        }

        public void Destroy(bool immediateMode = false) {
            if(Application.isPlaying && !immediateMode) {
                Destroy(gameObject);
            }else {
                DestroyImmediate(gameObject);
            }
        }

        #endregion

        #endregion

    }

}
