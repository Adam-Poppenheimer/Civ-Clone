using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.Common;
using Assets.UI.HexMap;
using Assets.UI.SocialPolicies;
using Assets.UI.Technology;

namespace Assets.UI.StateMachine {

    public class StateMachineInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private Animator              StateMachineAnimator  = null;
        [SerializeField] private LoadGameDisplay       LoadGameDisplay       = null;
        [SerializeField] private GameObject            OptionsDisplay        = null;
        [SerializeField] private GameCamera            GameCamera            = null;
        [SerializeField] private CellHoverDisplay      CellHoverDisplay      = null;
        [SerializeField] private SocialPoliciesDisplay SocialPoliciesDisplay = null;
        [SerializeField] private TechTreeDisplay       TechTreeDisplay       = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<Animator>().WithId("UI Animator").FromInstance(StateMachineAnimator);

            Container.Bind<LoadGameDisplay>().FromInstance(LoadGameDisplay);

            Container.Bind<GameObject>()
                .WithId("Options Display")
                .FromInstance(OptionsDisplay);

            Container.Bind<IGameCamera>().To<GameCamera>().FromInstance(GameCamera);

            Container.Bind<CellHoverDisplay>     ().FromInstance(CellHoverDisplay);
            Container.Bind<SocialPoliciesDisplay>().FromInstance(SocialPoliciesDisplay);
            Container.Bind<TechTreeDisplay>      ().FromInstance(TechTreeDisplay);

            Container.Bind<UIStateMachineBrain>().AsSingle();

            foreach(var behaviour in StateMachineAnimator.GetBehaviours<StateMachineBehaviour>()) {
                Container.Rebind(behaviour.GetType()).FromInstance(behaviour);

                Container.QueueForInject(behaviour);
            }
        }

        #endregion

        #endregion

    }

}
