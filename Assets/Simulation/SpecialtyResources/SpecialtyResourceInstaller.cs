﻿using System;
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

            var allResources = new List<ISpecialtyResourceDefinition>(Resources.LoadAll<SpecialtyResourceDefinition>("Specialty Resources"));

            Container.Bind<IEnumerable<ISpecialtyResourceDefinition>>()
                .WithId("Available Specialty Resources")
                .FromInstance(allResources.Cast<ISpecialtyResourceDefinition>());

            Container.Bind<IResourceNodeFactory>().To<ResourceNodeFactory>().AsSingle();

            Container.Bind<SpecialtyResourceSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
