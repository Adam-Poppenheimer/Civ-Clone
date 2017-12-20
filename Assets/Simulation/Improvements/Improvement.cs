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

        public bool IsComplete {
            get {
                return WorkInvested >= Template.WorkToComplete;
            }
        }

        public IImprovementTemplate Template { get; set; }

        public float WorkInvested {
            get { return _workInvested; }
            set {
                _workInvested = value;
                Debug.LogFormat("Work invested: {0}", _workInvested);
            }
        }
        private float _workInvested;

        #endregion

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies() {

        }

        #endregion

    }

}
