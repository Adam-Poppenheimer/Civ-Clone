using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;

using Assets.UI.Core;

namespace Assets.UI.StateMachine {

    public class UIStateMachineBrain {

        #region static fields and properties

        private static string ReturnTriggerName = "Return Requested";

        private static string CityTriggerName = "City State Requested";
        private static string UnitTriggerName = "Unit State Requested";
        private static string CellTriggerName = "Cell State Requested";

        private static string EscapeMenuTriggerName = "Escape Menu Requested";

        #endregion

        #region instance fields and properties

        public ICity    LastCityClicked { get; set; }
        public IUnit    LastUnitClicked { get; set; }
        public IHexCell LastCellClicked { get; set; }

        private IDisposable CityClickedSubscription;
        private IDisposable UnitClickedSubscription;

        private IDisposable CancelPressedSubscription;
        private IDisposable ClickedAnywhereSubscription;

        private IDisposable EscapeMenuRequestedSubscription;




        private Animator Animator;

        private CompositeCitySignals CompositeCitySignals;
        private CompositeUnitSignals CompositeUnitSignals;
        private HexCellSignals       CellSignals;
        private PlayerSignals        PlayerSignals;

        private GameCamera GameCamera;

        #endregion

        #region constructors

        [Inject]
        public UIStateMachineBrain(
            [Inject(Id = "UI Animator")] Animator animator, CompositeCitySignals compositeCitySignals,
            CompositeUnitSignals compositeUnitSignals, HexCellSignals cellSignals,
            PlayerSignals playerSignals, GameCamera gameCamera, CoreSignals coreSignals
        ) {
            Animator             = animator;
            CompositeCitySignals = compositeCitySignals;
            CompositeUnitSignals = compositeUnitSignals;
            CellSignals          = cellSignals;
            PlayerSignals        = playerSignals;
            GameCamera           = gameCamera;
            
            coreSignals.TurnBeganSignal.Subscribe(OnTurnBegan);
        }

        #endregion

        #region instance methods

        public void ClearListeners() {
            if(CancelPressedSubscription   != null) { CancelPressedSubscription  .Dispose(); }
            if(ClickedAnywhereSubscription != null) { ClickedAnywhereSubscription.Dispose(); }

            if(CityClickedSubscription != null) { CityClickedSubscription.Dispose(); }
            if(UnitClickedSubscription != null) { UnitClickedSubscription.Dispose(); }

            if(EscapeMenuRequestedSubscription != null) { EscapeMenuRequestedSubscription.Dispose(); }

            CellSignals.ClickedSignal.Unlisten(OnCellClicked);

            foreach(var parameter in Animator.parameters) {
                if(parameter.type == AnimatorControllerParameterType.Trigger) {
                    Animator.ResetTrigger(parameter.name);
                }
            }
        }

        public void ListenForTransitions(params TransitionType[] types) {
            foreach(var type in types) {
                ListenForTransition(type);
            }
        }

        public void ListenForTransition(TransitionType type) {
            if(type == TransitionType.ReturnViaButton) {
                CancelPressedSubscription   = PlayerSignals.CancelPressedSignal  .Subscribe(OnCancelPressed);

            }else if(type == TransitionType.ReturnViaClick) {
                ClickedAnywhereSubscription = PlayerSignals.ClickedAnywhereSignal.Subscribe(OnClickedAnywhere);

            } else if(type == TransitionType.ToCitySelected) {
                CityClickedSubscription = CompositeCitySignals.ActiveCivCityClickedSignal.Subscribe(OnCityClicked);

            }else if(type == TransitionType.ToUnitSelected) {
                UnitClickedSubscription = CompositeUnitSignals.ActiveCivUnitClickedSignal.Subscribe(OnUnitClicked);

            }else if(type == TransitionType.ToCellSelected) {
                CellSignals.ClickedSignal.Listen(OnCellClicked);

            }else if(type == TransitionType.ToEscapeMenu) {
                EscapeMenuRequestedSubscription = PlayerSignals.CancelPressedSignal.Subscribe(OnEscapeMenuRequested);
            }
        }

        public void EnableCameraMovement() {
            GameCamera.enabled = true;
        }

        public void DisableCameraMovement() {
            GameCamera.enabled = false;
        }

        private void OnCancelPressed(UniRx.Unit unit) {
            Animator.SetTrigger(ReturnTriggerName);
        }

        private void OnClickedAnywhere(PointerEventData eventData) {
            if(ClickedAnywhereTransitionLogic.ShouldClickCauseReturn(eventData)) {
                Animator.SetTrigger(ReturnTriggerName);
            }
        }

        private void OnCityClicked(ICity city) {
            LastCityClicked = city;
            Animator.SetTrigger(CityTriggerName);
        }

        private void OnUnitClicked(IUnit unit) {
            LastUnitClicked = unit;
            Animator.SetTrigger(UnitTriggerName);
        }

        private void OnCellClicked(IHexCell cell, Vector3 position) {
            LastCellClicked = cell;
            Animator.SetTrigger(CellTriggerName);
        }

        private void OnEscapeMenuRequested(UniRx.Unit unit) {
            Animator.SetTrigger(EscapeMenuTriggerName);
        }

        private void OnTurnBegan(ICivilization activeCiv) {
            foreach(var parameter in Animator.parameters) {
                if(parameter.type == AnimatorControllerParameterType.Trigger) {
                    Animator.ResetTrigger(parameter.name);
                }
            }
        }

        #endregion

    }

}