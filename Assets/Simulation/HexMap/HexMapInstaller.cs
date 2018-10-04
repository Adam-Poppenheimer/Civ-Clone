using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Util;

namespace Assets.Simulation.HexMap {

    public class HexMapInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private HexGrid Grid;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IHexGrid>().To<HexGrid>().FromInstance(Grid);

            var renderConfig = Resources.Load<HexMapRenderConfig>("Hex Map/Hex Map Render Config");
            Container.Bind<IHexMapRenderConfig>().To<HexMapRenderConfig>().FromInstance(renderConfig);

            var simulationConfig = Resources.Load<HexMapSimulationConfig>("Hex Map/Hex Map Simulation Config");
            Container.Bind<IHexMapSimulationConfig>().To<HexMapSimulationConfig>().FromInstance(simulationConfig);
            
            var featureConfig = Resources.Load<FeatureConfig>("Hex Map/Feature Config");
            Container.Bind<IFeatureConfig>().To<FeatureConfig>().FromInstance(featureConfig);

            Container.Bind<ICellYieldLogic>        ().To<CellYieldLogic>        ().AsSingle();
            Container.Bind<INoiseGenerator>        ().To<NoiseGenerator>        ().AsSingle();
            Container.Bind<IRiverCanon>            ().To<RiverCanon>            ().AsSingle();
            Container.Bind<IFreshWaterLogic>       ().To<FreshWaterLogic>       ().AsSingle();
            Container.Bind<ICellModificationLogic> ().To<CellModificationLogic> ().AsSingle();
            Container.Bind<IInherentCellYieldLogic>().To<InherentCellYieldLogic>().AsSingle();
            Container.Bind<IHexEdgeTypeLogic>      ().To<HexEdgeTypeLogic>      ().AsSingle();
            Container.Bind<IHexPathfinder>         ().To<HexPathfinder>         ().AsSingle();

            Container.Bind<MeshWelder>().AsSingle();

            Container.Bind<HexCellSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
