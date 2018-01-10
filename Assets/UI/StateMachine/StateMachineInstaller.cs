using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.UI.Cities;
using Assets.UI.Civilizations;
using Assets.UI.HexMap;
using Assets.UI.Units;

using Assets.UI.StateMachine.States;
using Assets.UI.StateMachine.Transitions;

namespace Assets.UI.StateMachine {

    public class StateMachineInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private Animator StateMachineAnimator;

        [SerializeField] private List<CityDisplayBase>         AllCityDisplays;
        [SerializeField] private List<CivilizationDisplayBase> AllCivilizationDisplays;
        [SerializeField] private List<TileDisplayBase>         AllTileDisplays;
        [SerializeField] private List<UnitDisplayBase>         AllUnitDisplays;

        [SerializeField] private List<RectTransform> DefaultPanels;
        
        [SerializeField] private HexMapEditor MapEditor;

        private TileUIState    TileUIState    = null;
        private CityUIState    CityUIState    = null;
        private DefaultUIState DefaultUIState = null;
        private UnitUIState    UnitUIState    = null;
        private MapEditorState MapEditorState = null;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<Animator>().WithId("UI State Machine Animator").FromInstance(StateMachineAnimator);

            Container.Bind<DefaultTransitionLogic>   ().AsSingle().NonLazy();
            Container.Bind<CityTransitionLogic>      ().AsSingle().NonLazy();
            Container.Bind<MapTileTransitionLogic>   ().AsSingle().NonLazy();
            Container.Bind<UnitTransitionLogic>      ().AsSingle().NonLazy();
            Container.Bind<MapEditingTransitionLogic>().AsSingle().NonLazy();

            Container.Bind<List<CityDisplayBase>>        ().FromInstance(AllCityDisplays);
            Container.Bind<List<CivilizationDisplayBase>>().FromInstance(AllCivilizationDisplays);
            Container.Bind<List<TileDisplayBase>>        ().FromInstance(AllTileDisplays);
            Container.Bind<List<UnitDisplayBase>>        ().FromInstance(AllUnitDisplays);

            Container.Bind<List<RectTransform>>().WithId("Default Panels").FromInstance(DefaultPanels);

            Container.Bind<HexMapEditor>().FromInstance(MapEditor);

            Container.Bind<TileUIState>()   .FromInstance(TileUIState);
            Container.Bind<CityUIState>()   .FromInstance(CityUIState);
            Container.Bind<DefaultUIState>().FromInstance(DefaultUIState);
            Container.Bind<UnitUIState>()   .FromInstance(UnitUIState);

            foreach(var behaviour in StateMachineAnimator.GetBehaviours<StateMachineBehaviour>()) {
                Container.Rebind(behaviour.GetType()).FromInstance(behaviour);

                Container.QueueForInject(behaviour);
            }
        }

        #endregion

        #endregion

    }

}
