using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.AI {

    public class AIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private AIConfig AIConfig;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IAIConfig>().To<AIConfig>().FromInstance(AIConfig);

            Container.Bind<IUnitStrengthEstimator>().To<UnitStrengthEstimator>().AsSingle();
            Container.Bind<IUnitCommandExecuter>  ().To<UnitCommandExecuter>  ().AsSingle();
        }

        #endregion

        #endregion

    }

}
