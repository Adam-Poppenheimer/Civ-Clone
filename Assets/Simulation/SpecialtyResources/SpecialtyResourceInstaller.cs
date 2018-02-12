using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.SpecialtyResources {

    public class SpecialtyResourceInstaller : MonoInstaller {

        #region instance fields and properties

        

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().To<ResourceNodeLocationCanon>().AsSingle();

            Container
                .Bind<ISpecialtyResourcePossessionCanon>()
                .To  <SpecialtyResourcePossessionCanon >()
                .AsSingle();

            Container
                .Bind<IResourceAssignmentCanon>()
                .To  <ResourceAssignmentCanon >()
                .AsSingle();

            var allResources = new List<ISpecialtyResourceDefinition>(Resources.LoadAll<SpecialtyResourceDefinition>("Specialty Resources"));

            Container.Bind<IEnumerable<ISpecialtyResourceDefinition>>()
                .WithId("All Speciality Resources")
                .FromInstance(allResources.Cast<ISpecialtyResourceDefinition>());

            Container.Bind<IResourceNodeFactory>().To<ResourceNodeFactory>().AsSingle();
        }

        #endregion

        #endregion

    }

}
