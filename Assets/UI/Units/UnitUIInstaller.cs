using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.Units {

    public class UnitUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject AbilityDisplayPrefab;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.BindMemoryPool<IAbilityDisplay, AbilityDisplayMemoryPool>()
                .To<AbilityDisplay>()
                .FromComponentInNewPrefab(AbilityDisplayPrefab);
        }

        #endregion

        #endregion

    }

}
