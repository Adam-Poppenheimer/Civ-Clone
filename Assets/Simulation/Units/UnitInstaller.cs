using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units {

    public class UnitInstaller : MonoInstaller {

        #region instance fields and properties

        private GameObject UnitPrefab;

        [SerializeField] private List<TerrainType> LandTerrainTypes;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IUnitConfig>().To<UnitConfig>().FromResource("Units");

            Container.Bind<GameObject>().WithId("Unit Prefab").FromResource("Units/Unit Prefab");

            var unitTemplates = new List<IUnitTemplate>();

            unitTemplates.AddRange(Resources.LoadAll<UnitTemplate>("Units"));

            Container.Bind<IEnumerable<IUnitTemplate>>()
                .WithId("Available Unit Templates")
                .FromInstance(unitTemplates);

            Container.Bind<IUnitFactory>().To<UnitFactory>().AsSingle();

            Container.Bind<IUnitPositionCanon>          ().To<UnitPositionCanon>          ().AsSingle();
            Container.Bind<IUnitProductionValidityLogic>().To<UnitProductionValidityLogic>().AsSingle();
            Container.Bind<IUnitTerrainCostLogic>       ().To<UnitTerrainCostLogic>       ().AsSingle();
            Container.Bind<IUnitLineOfSightLogic>       ().To<UnitLineOfSightLogic>       ().AsSingle();
            Container.Bind<ICombatInfoLogic>            ().To<CombatInfoLogic>            ().AsSingle();
            Container.Bind<ICityConquestLogic>          ().To<CityConquestLogic>          ().AsSingle();
            Container.Bind<IPostCombatMovementLogic>    ().To<PostCombatMovementLogic>    ().AsSingle();
            Container.Bind<ICombatDestructionLogic>     ().To<CombatDestructionLogic>     ().AsSingle();
            Container.Bind<ICombatExecuter>             ().To<CombatExecuter>             ().AsSingle();

            Container.Bind<UnitResponder>  ().AsSingle().NonLazy();

            Container.Bind<CompositeUnitSignals>().AsSingle().NonLazy();
            Container.Bind<UnitSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
