using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    public class ResourceInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private Transform ResourceNodeContainer;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<Transform>().WithId("Resource Node Container").FromInstance(ResourceNodeContainer);

            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().To<ResourceNodeLocationCanon>().AsSingle();

            var allResources = new List<IResourceDefinition>(Resources.LoadAll<ResourceDefinition>("Map Resources"));

            Container.Bind<IEnumerable<IResourceDefinition>>()
                .WithId("Available Resources")
                .FromInstance(allResources.Cast<IResourceDefinition>());

            Container.Bind<IResourceRestrictionLogic>().To<ResourceRestrictionLogic>().AsSingle();
            Container.Bind<IResourceNodeFactory>     ().To<ResourceNodeFactory>     ().AsSingle();
            Container.Bind<IResourceNodeYieldLogic>  ().To<ResourceNodeYieldLogic>  ().AsSingle();

            Container.Bind<ResourceSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
