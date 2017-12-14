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

        public IImprovementTemplate Template { get; private set; }

        public float WorkInvested { get; set; }

        #endregion

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IImprovementTemplate template) {
            Template = template;
        }

        #endregion

    }

}
