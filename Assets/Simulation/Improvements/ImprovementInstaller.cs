using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject ImprovementPrefab;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<GameObject>().WithId("Improvement Prefab").FromInstance(ImprovementPrefab);

            Container.Bind<IPossessionRelationship<IMapTile, IImprovement>>().To<ImprovementLocationCanon>().AsSingle();

            Container.Bind<IImprovementValidityLogic>().To<ImprovementValidityLogic>().AsSingle();
        }

        #endregion

        #endregion

    }

}
