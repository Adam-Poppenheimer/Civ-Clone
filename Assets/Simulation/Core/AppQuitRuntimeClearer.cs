using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapManagement;

namespace Assets.Simulation.Core {

    public class AppQuitRuntimeClearer : MonoBehaviour {

        #region instance fields and properties

        private IMapComposer MapComposer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IMapComposer mapComposer) {
            MapComposer = mapComposer;
        }

        #region Unity messages

        private void OnApplicationQuit() {
            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
