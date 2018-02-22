using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine {

    public class TitleScreenInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject    TitleScreenStateSelectionDisplay;
        [SerializeField] private GameObject    NewGameDisplay;
        [SerializeField] private RectTransform TitleScreenContainer;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<GameObject>()
                .WithId("Title Screen State Selection Display")
                .FromInstance(TitleScreenStateSelectionDisplay);

            Container.Bind<GameObject>()
                .WithId("New Game Display")
                .FromInstance(NewGameDisplay);

            Container.Bind<RectTransform>()
                .WithId("Title Screen Container")
                .FromInstance(TitleScreenContainer);
        }

        #endregion

        #endregion

    }

}
