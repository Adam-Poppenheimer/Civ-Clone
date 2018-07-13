using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapGeneration {

    public class MapGenerationInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapGenerationConfig Config;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IRiverGenerator>().To<RiverGenerator>().AsSingle();

            Container.Bind<IHexMapGenerator>().To<HexMapGenerator>().AsSingle();

            Container.Bind<IMapGenerationConfig>().To<MapGenerationConfig>().FromInstance(Config);
        }

        #endregion

        #endregion

    }

}
