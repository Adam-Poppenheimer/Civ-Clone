using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.TitleScreen;

namespace Assets.UI.StateMachine {

    public class TitleScreenInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject     TitleScreenStateSelectionDisplay;
        [SerializeField] private RectTransform  TitleScreenContainer;
        [SerializeField] private NewGameDisplay NewGameDisplay;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<GameObject>()
                .WithId("Title Screen State Selection Display")
                .FromInstance(TitleScreenStateSelectionDisplay);

            Container.Bind<RectTransform>()
                .WithId("Title Screen Container")
                .FromInstance(TitleScreenContainer);

            Container.Bind<NewGameDisplay>().FromInstance(NewGameDisplay);
        }

        #endregion

        #endregion

    }

}
