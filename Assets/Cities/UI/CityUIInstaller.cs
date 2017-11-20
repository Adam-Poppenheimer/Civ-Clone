using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Cities.UI {

    public class CityUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private CityUIConfig UIConfig;

        [SerializeField] private WorkerSlotDisplay SlotDisplayPrefab;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICityUIConfig>().To<CityUIConfig>().FromInstance(UIConfig);
            Container.Bind<IWorkerSlotDisplay>().To<WorkerSlotDisplay>().FromInstance(SlotDisplayPrefab);

        }

        #endregion

        #endregion

    }

}
