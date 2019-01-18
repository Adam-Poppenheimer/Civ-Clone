﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Barbarians {

    public class BarbariansInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private BarbarianConfig BarbarianConfig;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {   
            Container.Bind<IBarbarianConfig>().To<BarbarianConfig>().FromInstance(BarbarianConfig);
                     
            Container.Bind<IBarbarianGoalBrain>().To<BarbarianWanderBrain>         ().AsSingle();
            Container.Bind<IBarbarianGoalBrain>().To<BarbarianGuardEncampmentBrain>().AsSingle();
            Container.Bind<IBarbarianGoalBrain>().To<BarbarianBePrisonersBrain>    ().AsSingle();
            Container.Bind<IBarbarianGoalBrain>().To<BarbarianPillageBrain>        ().AsSingle();
            Container.Bind<IBarbarianGoalBrain>().To<BarbarianCaptureCivilianBrain>().AsSingle();
            Container.Bind<IBarbarianGoalBrain>().To<BarbarianFleeBrain>           ().AsSingle();
            Container.Bind<IBarbarianGoalBrain>().To<BarbarianAttackBrain>         ().AsSingle();

            Container.Bind<IBarbarianUnitBrain>          ().To<BarbarianUnitBrain>          ().AsSingle();
            Container.Bind<IBarbarianBrainFilterLogic>   ().To<BarbarianBrainFilterLogic>   ().AsSingle();
            Container.Bind<IBarbarianUtilityLogic>       ().To<BarbarianUtilityLogic>       ().AsSingle();
            Container.Bind<IBarbarianBrainWeightLogic>   ().To<BarbarianBrainWeightLogic>   ().AsSingle();
            Container.Bind<IEncampmentFactory>           ().To<EncampmentFactory>           ().AsSingle();
            Container.Bind<IBarbarianAvailableUnitsLogic>().To<BarbarianAvailableUnitsLogic>().AsSingle();
            Container.Bind<IBarbarianEncampmentSpawner>  ().To<BarbarianEncampmentSpawner>  ().AsSingle();
            Container.Bind<IBarbarianUnitSpawner>        ().To<BarbarianUnitSpawner>        ().AsSingle();
            Container.Bind<IEncampmentLocationCanon>     ().To<EncampmentLocationCanon>     ().AsSingle();
            Container.Bind<IBarbarianSpawningTools>      ().To<BarbarianSpawningTools>      ().AsSingle();
            Container.Bind<IBarbarianTurnExecuter>       ().To<BarbarianTurnExecuter>       ().AsSingle();

            Container.Bind<EncampmentClearingResponder>().AsSingle().NonLazy();
        }

        #endregion

        #endregion

    }

}
