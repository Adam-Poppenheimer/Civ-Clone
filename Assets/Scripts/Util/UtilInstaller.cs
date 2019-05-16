using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Util {

    public class UtilInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IGeometry2D>().To<Geometry2D>().AsSingle();
        }

        #endregion

        #endregion

    }

}
