using System;
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

        [SerializeField] private List<ImprovementTemplate> AvailableTemplates;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<GameObject>().WithId("Improvement Prefab").FromInstance(ImprovementPrefab);

            Container.Bind<IImprovementLocationCanon>().To<ImprovementLocationCanon>().AsSingle();

            Container.Bind<IEnumerable<IImprovementTemplate>>()
                .WithId("Available Improvement Templates")
                .FromInstance(AvailableTemplates.Cast<IImprovementTemplate>());

            Container.Bind<IImprovementValidityLogic>().To<ImprovementValidityLogic>().AsSingle();

            Container.Bind<IImprovementFactory>().To<ImprovementFactory>().AsSingle();
        }

        #endregion

        #endregion

    }

}
