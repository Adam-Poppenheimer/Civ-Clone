using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Visibility;
using Assets.Simulation.Technology;

using Assets.UI.Civilizations;
using Assets.UI.Cities;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeDefaultState : StateMachineBehaviour {

        #region instance fields and properties

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();



        private List<CivilizationDisplayBase>            CivilizationDisplays;
        private CoreSignals                              CoreSignals;
        private List<RectTransform>                      DefaultPanels;
        private IGameCore                                GameCore;
        private UIStateMachineBrain                      Brain;
        private CitySummaryManager                       CitySummaryManager;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private IExplorationCanon                        ExplorationCanon;
        private VisibilitySignals                        VisibilitySignals;
        private RectTransform                            FreeTechsDisplay;
        private ITechCanon                               TechCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            List<CivilizationDisplayBase> civilizationDisplays, CoreSignals coreSignals,
            [Inject(Id = "Play Mode Default Panels")] List<RectTransform> defaultPanels,
            IGameCore gameCore, UIStateMachineBrain brain, CitySummaryManager citySummaryManager,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IExplorationCanon explorationCanon, VisibilitySignals visibilitySignals,
            [Inject(Id = "Free Techs Display")] RectTransform freeTechsDisplay, ITechCanon techCanon
        ){
            CivilizationDisplays = civilizationDisplays;
            CoreSignals          = coreSignals;
            DefaultPanels        = defaultPanels;
            GameCore             = gameCore;
            Brain                = brain;
            CitySummaryManager   = citySummaryManager;
            CityLocationCanon    = cityLocationCanon;
            ExplorationCanon     = explorationCanon;
            VisibilitySignals    = visibilitySignals;
            FreeTechsDisplay     = freeTechsDisplay;
            TechCanon            = techCanon;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.ObjectToDisplay = GameCore.ActiveCivilization;
                display.gameObject.SetActive(true);
                display.Refresh();
            }

            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(true);
            }

            Brain.ClearListeners();
            Brain.EnableCameraMovement();
            Brain.EnableCellHovering();

            Brain.ListenForTransitions(
                TransitionType.CitySelected, TransitionType.UnitSelected, TransitionType.ToEscapeMenu
            );

            CitySummaryManager.BuildSummaries();

            FreeTechsDisplay.gameObject.SetActive(TechCanon.GetFreeTechsForCiv(GameCore.ActiveCivilization) > 0);

            SignalSubscriptions.Add(CoreSignals      .TurnBeganSignal              .Subscribe(OnTurnBegan));
            SignalSubscriptions.Add(VisibilitySignals.CellBecameExploredByCivSignal.Subscribe(OnCellBecameExploredByCiv));
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CitySummaryManager.RepositionSummaries();

            FreeTechsDisplay.gameObject.SetActive(TechCanon.GetFreeTechsForCiv(GameCore.ActiveCivilization) > 0);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }

            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(false);
            }

            CitySummaryManager.ClearSummaries();

            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
            SignalSubscriptions.Clear();
            FreeTechsDisplay.gameObject.SetActive(false);
        }

        #endregion

        private void OnTurnBegan(ICivilization civ) {
            CitySummaryManager.ClearSummaries();
            CitySummaryManager.BuildSummaries();
        }

        private void OnCellBecameExploredByCiv(Tuple<IHexCell, ICivilization> data) {
            var cityAtCell = CityLocationCanon.GetPossessionsOfOwner(data.Item1).FirstOrDefault();

            if(cityAtCell != null && ExplorationCanon.IsCellExplored(data.Item1)) {
                CitySummaryManager.BuildSummaryForCity(cityAtCell);
            }
        }

        #endregion

    }

}
