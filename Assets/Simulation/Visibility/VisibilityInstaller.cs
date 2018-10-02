using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Visibility {

    public class VisibilityInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IVisibilityCanon>    ().To<VisibilityCanon>    ().AsSingle();
            Container.Bind<IVisibilityResponder>().To<VisibilityResponder>().AsSingle().NonLazy();

            Container.Bind<VisibilitySignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
