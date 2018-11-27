using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.SocialPolicies {

    public class SocialPolicyInstaller : MonoInstaller {

        #region instance fields and properties

        

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            var availableTrees = new List<IPolicyTreeDefinition>();
            availableTrees.AddRange(Resources.LoadAll<PolicyTreeDefinition>("Social Policies"));

            var availablePolicies = new List<ISocialPolicyDefinition>();
            availablePolicies.AddRange(Resources.LoadAll<SocialPolicyDefinition>("Social Policies"));

            Container.Bind<IEnumerable<IPolicyTreeDefinition>>()
                     .WithId("Available Policy Trees")
                     .FromInstance(availableTrees);

            Container.Bind<IEnumerable<ISocialPolicyDefinition>>()
                     .WithId("Available Policies")
                     .FromInstance(availablePolicies);

            Container.Bind<ISocialPolicyCanon>     ().To<SocialPolicyCanon>     ().AsSingle();
            Container.Bind<ISocialPolicyCostLogic> ().To<SocialPolicyCostLogic> ().AsSingle();
            Container.Bind<ISocialPolicyBonusLogic>().To<SocialPolicyBonusLogic>().AsSingle();

            Container.Bind<FreeBuildingsPolicyResponder>().AsSingle().NonLazy();
        }

        #endregion

        #endregion

    }

}
