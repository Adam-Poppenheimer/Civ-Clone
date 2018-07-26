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

        [SerializeField] private float MapTileHoverDelay;

        [SerializeField] private YieldSummaryDisplay MapTileHoverYieldDisplay;

        [SerializeField] private GameObject PathIndicatorPrefab;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<MonoBehaviour>().WithId("Coroutine Invoker").FromInstance(this);
            Container.Bind<float>().WithId("Map Tile Hover Delay").FromInstance(MapTileHoverDelay);            

            Container.BindInterfacesAndSelfTo<HexCellSignalLogic>().AsSingle();

            Container.Bind<IYieldSummaryDisplay>()
                .WithId("Tile Hover Yield Display")
                .To<YieldSummaryDisplay>()
                .FromInstance(MapTileHoverYieldDisplay);

            Container.Bind<ICellPathDrawer>().To<CellPathDrawer>().AsSingle();

            Container.Bind<HexCellOverlayManager>().AsSingle();
        }

        #endregion

        #endregion

    }

}
