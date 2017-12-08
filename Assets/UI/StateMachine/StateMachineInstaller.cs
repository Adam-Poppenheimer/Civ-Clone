using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.Cities;
using Assets.UI.Civilizations;
using Assets.UI.GameMap;

using Assets.UI.StateMachine.States;
using Assets.UI.StateMachine.Transitions;

namespace Assets.UI.StateMachine {

    public class StateMachineInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private Animator StateMachineAnimator;

        [SerializeField] private List<CityDisplayBase>         AllCityDisplays;
        [SerializeField] private List<CivilizationDisplayBase> AllCivilizationDisplays;
        [SerializeField] private List<TileDisplayBase>         AllTileDisplays;

        private TileUIState TileUIState = null;
        private CityUIState CityUIState = null;
        private DefaultUIState DefaultUIState = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<Animator>().WithId("UI State Machine Animator").FromInstance(StateMachineAnimator);

            Container.Bind<DefaultTransitionLogic>().AsSingle().NonLazy();
            Container.Bind<CityTransitionLogic>   ().AsSingle().NonLazy();
            Container.Bind<MapTileTransitionLogic>().AsSingle().NonLazy();

            Container.Bind<List<CityDisplayBase>>        ().FromInstance(AllCityDisplays);
            Container.Bind<List<CivilizationDisplayBase>>().FromInstance(AllCivilizationDisplays);
            Container.Bind<List<TileDisplayBase>>        ().FromInstance(AllTileDisplays);

            Container.Bind<TileUIState>().FromInstance(TileUIState);
            Container.Bind<CityUIState>().FromInstance(CityUIState);
            Container.Bind<DefaultUIState>().FromInstance(DefaultUIState);

            foreach(var behaviour in StateMachineAnimator.GetBehaviours<StateMachineBehaviour>()) {
                Container.Rebind(behaviour.GetType()).FromInstance(behaviour);

                Container.QueueForInject(behaviour);
            }
        }

        #endregion

        #endregion

    }

}
