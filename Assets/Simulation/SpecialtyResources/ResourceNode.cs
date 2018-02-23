﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.SpecialtyResources {

    public class ResourceNode : MonoBehaviour, IResourceNode {

        #region instance fields and properties

        #region from IResourceNode

        public int Copies { get; set; }

        public ISpecialtyResourceDefinition Resource { get; set; }

        #endregion

        private SpecialtyResourceSignals Signals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(SpecialtyResourceSignals signals) {
            Signals = signals;
        }

        #region Unity messages

        private void OnDestroy() {
            Signals.ResourceNodeBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #endregion

    }

}