using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Cities;
using Assets.GameMap;
using Assets.GameMap.UI;

namespace Assets.Core {

    public class CoreInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject CityPrefab;
        [SerializeField] private TileResourceConfig TileResourceConfig;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<GameObject>().WithId("City Prefab").FromInstance(CityPrefab);

            Container.BindFactory<IMapTile, ICity, Factory<IMapTile, ICity>>().FromFactory<RecordkeepingCityFactory>();

            Container.Bind<ITurnExecuter>().To<TurnExecuter>().AsSingle();
            Container.Bind<GameCore>().AsSingle();

            Container.Bind<ITileEventBroadcaster>().To<TileEventBroadcaster>().AsSingle();

            Container.Bind<ITileResourceLogic>().To<TileResourceLogic>().AsSingle();
            Container.Bind<ITileResourceConfig>().To<TileResourceConfig>().AsSingle();
        }

        #endregion

        #endregion

    }

}
