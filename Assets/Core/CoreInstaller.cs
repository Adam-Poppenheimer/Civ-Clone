using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Cities;

namespace Assets.Core {

    public class CoreInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject CityPrefab;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<GameObject>().WithId("City Prefab").FromInstance(CityPrefab);

            Container.BindFactory<ICity, Factory<ICity>>().FromFactory<RecordkeepingCityFactory>();

            Container.Bind<ITurnExecuter>().To<TurnExecuter>().AsSingle();
            Container.Bind<GameCore>().AsSingle();
        }

        #endregion

        #endregion

    }

}
