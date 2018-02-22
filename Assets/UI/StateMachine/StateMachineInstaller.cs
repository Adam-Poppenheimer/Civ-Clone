using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine {

    public class StateMachineInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private Animator   StateMachineAnimator;
        [SerializeField] private GameObject LoadSavedGameDisplay;
        [SerializeField] private GameObject OptionsDisplay;

        [SerializeField] private GameCamera GameCamera;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<Animator>().WithId("UI Animator").FromInstance(StateMachineAnimator);

            Container.Bind<GameObject>()
                .WithId("Load Saved Game Display")
                .FromInstance(LoadSavedGameDisplay);

            Container.Bind<GameObject>()
                .WithId("Options Display")
                .FromInstance(OptionsDisplay);

            Container.Bind<GameCamera>().FromInstance(GameCamera);

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
