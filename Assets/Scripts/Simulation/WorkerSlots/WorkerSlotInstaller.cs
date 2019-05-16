using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.WorkerSlots {

    public class WorkerSlotInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IWorkerSlotFactory>().To<WorkerSlotFactory>().AsSingle();

            Container.Bind<WorkerSlotSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
