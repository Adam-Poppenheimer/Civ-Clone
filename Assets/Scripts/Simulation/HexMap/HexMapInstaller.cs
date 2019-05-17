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

        [SerializeField] private HexGrid Grid = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IHexGrid>().To<HexGrid>().FromInstance(Grid);

            var simulationConfig = Resources.Load<HexMapSimulationConfig>("Hex Map/Hex Map Simulation Config");
            Container.Bind<IHexMapSimulationConfig>().To<HexMapSimulationConfig>().FromInstance(simulationConfig);

            Container.Bind<ICellYieldLogic>          ().To<CellYieldLogic>          ().AsSingle();
            Container.Bind<IRiverCanon>              ().To<RiverCanon>              ().AsSingle();
            Container.Bind<IFreshWaterLogic>         ().To<FreshWaterLogic>         ().AsSingle();
            Container.Bind<ICellModificationLogic>   ().To<CellModificationLogic>   ().AsSingle();
            Container.Bind<IInherentCellYieldLogic>  ().To<InherentCellYieldLogic>  ().AsSingle();
            Container.Bind<IHexPathfinder>           ().To<HexPathfinder>           ().AsSingle();
            Container.Bind<IRiverCornerValidityLogic>().To<RiverCornerValidityLogic>().AsSingle();

            Container.Bind<HexCellSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
