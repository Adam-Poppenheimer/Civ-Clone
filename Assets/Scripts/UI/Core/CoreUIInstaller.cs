using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using TMPro;

namespace Assets.UI.Core {

    public class CoreUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private DescriptionTooltip DescriptionTooltip = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IYieldFormatter>().To<YieldFormatter>().AsSingle();

            Container.Bind<DescriptionTooltip>().FromInstance(DescriptionTooltip);
        }

        #endregion

        #endregion

    }

}
