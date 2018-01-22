﻿using System;
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

using Assets.UI.Core;

namespace Assets.UI.StateMachine {

    public class UIStateMachineBrain {

        #region static fields and properties

        private static string ReturnTriggerName = "Return Requested";

        private static string CityTriggerName = "City State Requested";
        private static string UnitTriggerName = "Unit State Requested";
        private static string CellTriggerName = "Cell State Requested";

        #endregion

        #region instance fields and properties

        public ICity    LastCityClicked { get; private set; }
        public IUnit    LastUnitClicked { get; private set; }
        public IHexCell LastCellClicked { get; private set; }

        private IDisposable CityClickedSubscription;
        private IDisposable UnitClickedSubscription;

        private IDisposable CancelPressedSubscription;
        private IDisposable ClickedAnywhereSubscription;


        private Animator Animator;

        private CitySignals    CitySignals;
        private UnitSignals    UnitSignals;
        private HexCellSignals CellSignals;
        private PlayerSignals  PlayerSignals;

        #endregion

        #region constructors

        [Inject]
        public UIStateMachineBrain(
            [Inject(Id = "UI Animator")] Animator animator,
            CitySignals citySignals, UnitSignals unitSignals, HexCellSignals cellSignals,
            PlayerSignals playerSignals
        ) {
            Animator      = animator;
            CitySignals   = citySignals;
            UnitSignals   = unitSignals;
            CellSignals   = cellSignals;
            PlayerSignals = playerSignals;
        }

        #endregion

        #region instance methods

        public void ClearListeners() {
            if(CancelPressedSubscription   != null) { CancelPressedSubscription  .Dispose(); }
            if(ClickedAnywhereSubscription != null) { ClickedAnywhereSubscription.Dispose(); };

            if(CityClickedSubscription != null) { CityClickedSubscription.Dispose(); }
            if(UnitClickedSubscription != null) { UnitClickedSubscription.Dispose(); }

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
                CityClickedSubscription = CitySignals.ClickedSignal.Subscribe(OnCityClicked);

            }else if(type == TransitionType.ToUnitSelected) {
                UnitClickedSubscription = UnitSignals.ClickedSignal.Subscribe(OnUnitClicked);

            }else if(type == TransitionType.ToCellSelected) {
                CellSignals.ClickedSignal.Listen(OnCellClicked);
            }
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

        #endregion

    }

}