﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Units {

    public class UnitInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private List<CellTerrain> LandTerrainTypes;
        [SerializeField] private Transform UnitContainer;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IUnitConfig>().To<UnitConfig>().FromResource("Units");

            Container.Bind<GameObject>().WithId("Unit Prefab").FromResource("Units/Unit Prefab");

            var unitTemplates = new List<IUnitTemplate>();

            unitTemplates.AddRange(Resources.LoadAll<UnitTemplate>("Units"));

            foreach(var template in unitTemplates) {
                Container.QueueForInject(template);
            }

            Container.Bind<IEnumerable<IUnitTemplate>>()
                     .WithId("Available Unit Templates")
                     .FromInstance(unitTemplates);

            foreach(var promotion in Resources.LoadAll<Promotion>("Promotions")) {
                Container.QueueForInject(promotion);
            }

            Container.Bind<Transform>().WithId("Unit Container").FromInstance(UnitContainer);

            Container.Bind<IUnitFactory>().To<UnitFactory>().AsSingle();

            Container.Bind<IUnitPositionCanon>          ().To<UnitPositionCanon>          ().AsSingle();
            Container.Bind<IUnitProductionValidityLogic>().To<UnitProductionValidityLogic>().AsSingle();
            Container.Bind<IUnitLineOfSightLogic>       ().To<UnitLineOfSightLogic>       ().AsSingle();
            Container.Bind<ICombatInfoLogic>            ().To<CombatInfoLogic>            ().AsSingle();
            Container.Bind<ICombatExecuter>             ().To<CombatExecuter>             ().AsSingle();
            Container.Bind<ICombatAuraLogic>            ().To<CombatAuraLogic>            ().AsSingle();
            Container.Bind<IUnitHealingLogic>           ().To<UnitHealingLogic>           ().AsSingle();
            Container.Bind<ICanBuildCityLogic>          ().To<CanBuildCityLogic>          ().AsSingle();
            Container.Bind<IUnitFortificationLogic>     ().To<UnitFortificationLogic>     ().AsSingle();
            Container.Bind<IGreatPersonFactory>         ().To<GreatPersonFactory>         ().AsSingle();
            Container.Bind<ICityCombatModifierLogic>    ().To<CityCombatModifierLogic>    ().AsSingle();
            Container.Bind<IUnitModifiers>              ().To<UnitModifiers>              ().AsSingle();
            Container.Bind<IUnitPromotionLogic>         ().To<UnitPromotionLogic>         ().AsSingle();
            Container.Bind<IUnitGarrisonLogic>          ().To<UnitGarrisonLogic>          ().AsSingle();
            
            Container.Bind<IPostCombatResponder>().To<CityConquestResponder>  ().AsSingle();
            Container.Bind<IPostCombatResponder>().To<CombatDestructionLogic> ().AsSingle();
            Container.Bind<IPostCombatResponder>().To<PostCombatMovementLogic>().AsSingle();

            Container.Bind<UnitResponder>   ().AsSingle().NonLazy();
            Container.Bind<GoldRaidingLogic>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<FreeUnitsResponder>  ().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<FreeGreatPeopleCanon>().AsSingle();

            Container.Bind<CompositeUnitSignals>().AsSingle().NonLazy();
            Container.Bind<UnitSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
