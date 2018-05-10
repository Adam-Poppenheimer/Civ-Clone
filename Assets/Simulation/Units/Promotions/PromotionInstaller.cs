using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Units.Promotions {

    public class PromotionInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            var promotionTreeTemplates = Resources.LoadAll<PromotionTreeTemplate>("Promotions");
            var promotions             = Resources.LoadAll<Promotion>            ("Promotions");

            Container.Bind<IEnumerable<IPromotionTreeTemplate>>()
                     .WithId("Available Promotion Tree Templates")
                     .FromInstance(promotionTreeTemplates.Cast<IPromotionTreeTemplate>());

            Container.Bind<IEnumerable<IPromotion>>()
                     .WithId("Available Promotions")
                     .FromInstance(promotions.Cast<IPromotion>());

            Container.Bind<IPromotionParser>        ().To<PromotionParser>        ().AsSingle();
            Container.Bind<IUnitExperienceLogic>    ().To<UnitExperienceLogic>    ().AsSingle();
            Container.Bind<IStartingExperienceLogic>().To<StartingExperienceLogic>().AsSingle();
        }

        #endregion

        #endregion

    }

}
