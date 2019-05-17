using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class MapChunkInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private Transform FeatureContainer = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<Transform>().WithId("Feature Container").FromInstance(FeatureContainer);

            Container.Bind<IHexFeatureManager>   ().To<HexFeatureManager>   ().AsSingle();
            Container.Bind<IFeatureLocationLogic>().To<FeatureLocationLogic>().AsSingle();

            //The order here is important to make sure certain features take precedence over others
            Container.Bind<IFeaturePlacer>().To<CityFeaturePlacer>       ().AsSingle();
            Container.Bind<IFeaturePlacer>().To<EncampmentFeaturePlacer> ().AsSingle();
            Container.Bind<IFeaturePlacer>().To<ImprovementFeaturePlacer>().AsSingle();
            Container.Bind<IFeaturePlacer>().To<ResourceFeaturePlacer>   ().AsSingle();
            Container.Bind<IFeaturePlacer>().To<RuinsFeaturePlacer>      ().AsSingle();
            Container.Bind<IFeaturePlacer>().To<TreeFeaturePlacer>       ().AsSingle();
        }

        #endregion

        #endregion

    }

}
