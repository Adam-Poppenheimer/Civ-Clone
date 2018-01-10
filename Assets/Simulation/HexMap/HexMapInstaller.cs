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

        [SerializeField] private HexGridConfig Config;

        [SerializeField] private Texture2D NoiseSource;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IHexGrid>().To<HexGrid>().FromInstance(Grid);

            Container.Bind<IHexGridConfig>().To<HexGridConfig>().FromInstance(Config);

            Container.Bind<ITileResourceLogic>().To<TileResourceLogic>().AsSingle();

            Container.Bind<Texture2D>().WithId("Noise Source").FromInstance(NoiseSource);
            Container.Bind<int>()      .WithId("Random Seed") .FromInstance(Config.RandomSeed);

            Container.Bind<INoiseGenerator>().To<NoiseGenerator>().AsSingle();

            Container.DeclareSignal<CellClickedSignal>();
            Container.DeclareSignal<CellPointerEnterSignal>();
            Container.DeclareSignal<CellPointerExitSignal>();

            Container.Bind<HexCellSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
