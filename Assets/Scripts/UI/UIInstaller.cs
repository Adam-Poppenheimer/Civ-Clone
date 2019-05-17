using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.Common;

namespace Assets.UI {

    public class UIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapGenerationDisplay MapGenerationDisplay = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICameraFocuser>      ().To<CameraFocuser>().AsSingle();
            Container.Bind<MapGenerationDisplay>()                    .FromInstance(MapGenerationDisplay);
        }

        #endregion

        #endregion

    }
}
