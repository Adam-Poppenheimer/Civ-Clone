using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Technology {

    public class TechnologyInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private List<TechDefinition> AvailableTechs;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<List<ITechDefinition>>().WithId("Available Techs").FromInstance(AvailableTechs.Cast<ITechDefinition>().ToList());

            Container.Bind<ITechCanon>().To<TechCanon>().AsSingle();
        }

        #endregion

        #endregion

    }

}
