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

                var appearancePrefab = Instantiate(_improvementTemplate.AppearancePrefab);
                appearancePrefab.transform.SetParent(transform, false);
            }
        }
        private IImprovementTemplate _improvementTemplate;

        #endregion

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies() {

        }

        #endregion

    }

}
