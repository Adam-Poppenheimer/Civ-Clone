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

        public IImprovementTemplate Template {
            get { return _improvementTemplate; }
            set {
                _improvementTemplate = value;

                if(_improvementTemplate.AppearancePrefab != null) {
                    var appearancePrefab = Instantiate(_improvementTemplate.AppearancePrefab);
                    appearancePrefab.transform.SetParent(transform, false);
                }
            }
        }
        private IImprovementTemplate _improvementTemplate;

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
            Signals.ImprovementBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #endregion

    }

}
