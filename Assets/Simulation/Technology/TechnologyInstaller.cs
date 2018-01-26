using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Technology {

    public class TechnologyInstaller : MonoInstaller {

        #region instance fields and properties

        

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            var allTechs = new List<ITechDefinition>(Resources.LoadAll<TechDefinition>("Techs"));

            Container.Bind<List<ITechDefinition>>().WithId("Available Techs").FromInstance(allTechs);

            Container.Bind<ITechCanon>().To<TechCanon>().AsSingle();
        }

        #endregion

        #endregion

    }

}
