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

        [SerializeField] private float               MapTileHoverDelay        = 0f;
        [SerializeField] private YieldSummaryDisplay MapTileHoverYieldDisplay = null;
        [SerializeField] private HexCellOverlay      HexCellOverlayPrefab     = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<float>().WithId("Map Tile Hover Delay").FromInstance(MapTileHoverDelay);            

            Container.BindInterfacesAndSelfTo<HexCellSignalLogic>().AsSingle();

            Container.Bind<IYieldSummaryDisplay>()
                .WithId("Tile Hover Yield Display")
                .To<YieldSummaryDisplay>()
                .FromInstance(MapTileHoverYieldDisplay);

            Container.Bind<ICellPathDrawer>().To<CellPathDrawer>().AsSingle();

            Container.Bind<IHexCellOverlayManager>().To<HexCellOverlayManager>().AsSingle();

            Container.BindMemoryPool<HexCellOverlay, HexCellOverlay.Pool>()
                     .WithInitialSize(20)
                     .FromComponentInNewPrefab(HexCellOverlayPrefab)
                     .UnderTransformGroup("HexCell Overlays");
        }

        #endregion

        #endregion

    }

}
