﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject ImprovementPrefab;
        [SerializeField] private GameObject ImprovementSitePrefab;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<GameObject>().WithId("Improvement Prefab")     .FromInstance(ImprovementPrefab);
            Container.Bind<GameObject>().WithId("Improvement Site Prefab").FromInstance(ImprovementSitePrefab);

            Container.Bind<IImprovementLocationCanon>().To<ImprovementLocationCanon>().AsSingle();

            var allImprovements = new List<IImprovementTemplate>(Resources.LoadAll<ImprovementTemplate>("Improvements"));

            Container.Bind<IEnumerable<IImprovementTemplate>>()
                .WithId("Available Improvement Templates")
                .FromInstance(allImprovements);

            Container.Bind<IImprovementValidityLogic>       ().To<ImprovementValidityLogic>       ().AsSingle();
            Container.Bind<IImprovementYieldLogic>          ().To<ImprovementYieldLogic>          ().AsSingle();
            Container.Bind<IImprovementWorkLogic>           ().To<ImprovementWorkLogic>           ().AsSingle();
            Container.Bind<IImprovementDamageExecuter>      ().To<ImprovementDamageExecuter>      ().AsSingle();
            Container.Bind<IImprovementConstructionExecuter>().To<ImprovementConstructionExecuter>().AsSingle();

            Container.Bind<IImprovementFactory>().To<ImprovementFactory>().AsSingle();

            Container.Bind<ImprovementSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
