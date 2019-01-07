using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.AI {

    public class AIInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IUnitStrengthEstimator>().To<UnitStrengthEstimator>().AsSingle();
            Container.Bind<IUnitCommandExecuter>  ().To<UnitCommandExecuter>  ().AsSingle();
        }

        #endregion

        #endregion

    }

}
