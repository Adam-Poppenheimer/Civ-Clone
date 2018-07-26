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

        

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().To<ResourceNodeLocationCanon>().AsSingle();

            var allResources = new List<IResourceDefinition>(Resources.LoadAll<ResourceDefinition>("Specialty Resources"));

            Container.Bind<IEnumerable<IResourceDefinition>>()
                .WithId("Available Specialty Resources")
                .FromInstance(allResources.Cast<IResourceDefinition>());

            Container.Bind<IResourceNodeFactory>().To<ResourceNodeFactory>().AsSingle();

            Container.Bind<ResourceSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
