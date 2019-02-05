using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class MapRenderingInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapRenderConfig RenderConfig;
        [SerializeField] private FeatureConfig      FeatureConfig;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IMapRenderConfig>().To<MapRenderConfig>().FromInstance(RenderConfig);
            Container.Bind<IFeatureConfig>  ().To<FeatureConfig>  ().FromInstance(FeatureConfig);

            Container.Bind<ITerrainAlphamapLogic>().To<TerrainAlphamapLogic>().AsSingle();
        }

        #endregion

        #endregion

    }

}
