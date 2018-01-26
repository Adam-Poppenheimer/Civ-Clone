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

        [SerializeField] private Texture2D NoiseSource;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IHexGrid>().To<HexGrid>().FromInstance(Grid);

            var config = Resources.Load<HexGridConfig>("Hex Map/Hex Grid Config");

            Container.Bind<IHexGridConfig>().To<HexGridConfig>().FromInstance(config);

            Container.Bind<ITileResourceLogic>().To<TileResourceLogic>().AsSingle();

            Container.Bind<Texture2D>().WithId("Noise Source").FromInstance(NoiseSource);
            Container.Bind<int>()      .WithId("Random Seed") .FromInstance(config.RandomSeed);

            Container.Bind<INoiseGenerator>().To<NoiseGenerator>().AsSingle();

            Container.Bind<IRiverCanon>().To<RiverCanon>().AsSingle();

            Container.DeclareSignal<CellClickedSignal>();
            Container.DeclareSignal<CellPointerEnterSignal>();
            Container.DeclareSignal<CellPointerExitSignal>();

            Container.Bind<HexCellSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
