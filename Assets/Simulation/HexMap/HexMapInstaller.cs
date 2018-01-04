using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexMapInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private HexGrid Grid;

        [SerializeField] private HexMesh Mesh;

        [SerializeField] private TileConfig Config;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IHexGrid>().To<HexGrid>().FromInstance(Grid);

            Container.Bind<HexMesh>().FromInstance(Mesh);

            Container.Bind<ITileConfig>().To<TileConfig>().FromInstance(Config);

            Container.Bind<ITileResourceLogic>().To<TileResourceLogic>().AsSingle();

            Container.DeclareSignal<CellClickedSignal>();
            Container.DeclareSignal<CellPointerEnterSignal>();
            Container.DeclareSignal<CellPointerExitSignal>();

            Container.Bind<HexCellSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
