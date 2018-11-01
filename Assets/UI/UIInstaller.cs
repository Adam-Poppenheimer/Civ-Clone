using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.UI {

    public class UIInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICameraFocuser>().To<CameraFocuser>().AsSingle();
        }

        #endregion

        #endregion

    }
}
