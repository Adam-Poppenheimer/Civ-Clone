using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Units.Promotions {

    public class PromotionInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IPromotionParser>        ().To<PromotionParser>        ().AsSingle();
            Container.Bind<IUnitExperienceLogic>    ().To<UnitExperienceLogic>    ().AsSingle();
            Container.Bind<IStartingExperienceLogic>().To<StartingExperienceLogic>().AsSingle();
        }

        #endregion

        #endregion

    }

}
