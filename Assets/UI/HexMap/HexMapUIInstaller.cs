using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

namespace Assets.UI.HexMap {

    public class HexMapUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapTileDisplay MapTileDisplay;

        [SerializeField] private float MapTileHoverDelay;

        [SerializeField] private ResourceSummaryDisplay MapTileHoverYieldDisplay;

        [SerializeField] private GameObject PathIndicatorPrefab;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<MonoBehaviour>().WithId("Coroutine Invoker").FromInstance(this);
            Container.Bind<float>().WithId("Map Tile Hover Delay").FromInstance(MapTileHoverDelay);            

            Container.BindInterfacesAndSelfTo<HexCellSignalLogic>().AsSingle();

            Container.Bind<IResourceSummaryDisplay>()
                .WithId("Tile Hover Yield Display")
                .To<ResourceSummaryDisplay>()
                .FromInstance(MapTileHoverYieldDisplay);

            Container.BindMemoryPool<PathIndicator, PathIndicator.MemoryPool>().FromComponentInNewPrefab(PathIndicatorPrefab);

            Container.Bind<ITilePathDrawer>().To<TilePathDrawer>().AsSingle();
        }

        #endregion

        #endregion

    }

}
