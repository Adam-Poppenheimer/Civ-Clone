using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.Core {

    public class CoreUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private Camera MainCamera;

        [SerializeField] private CameraConfig CameraConfig;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICameraConfig>().To<CameraConfig>().FromInstance(CameraConfig);
            Container.Bind<Camera>().WithId("Main Camera").FromInstance(MainCamera);

            Container.BindInterfacesAndSelfTo<CameraLogic>().AsSingle().NonLazy();
        }

        #endregion

        #endregion

    }

}
