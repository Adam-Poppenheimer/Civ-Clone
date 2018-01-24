﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

using Assets.UI.Units;
using Assets.UI.HexMap;

namespace Assets.UI.StateMachine.States.PlayMode.Unit {

    public class DefaultUnitState : StateMachineBehaviour {

        #region instance fields and properties
        
        private IUnit SelectedUnit;

        private IUnit UnitToAttack;

        private ICity CityToAttack;

        private IHexCell ProspectiveTravelGoal;

        private List<IHexCell> ProspectivePath;

        private bool IsDragging;

        private List<IDisposable> EventSubscriptions = new List<IDisposable>();




        private List<UnitDisplayBase> DisplaysToManage;

        private UIStateMachineBrain Brain;

        private ICombatExecuter CombatExecuter;

        private IUnitPositionCanon UnitPositionCanon;

        private ICityFactory CityFactory;

        private IHexGrid Grid;

        private IUnitTerrainCostLogic TerrainCostLogic;

        private ITilePathDrawer PathDrawer;

        private CitySignals CitySignals;

        private UnitSignals UnitSignals;

        private HexCellSignals CellSignals;

        private CombatSummaryDisplay CombatSummaryDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            List<UnitDisplayBase> displaysToManage, UIStateMachineBrain brain,
            ICombatExecuter combatExecuter, IUnitPositionCanon unitPositionCanon,
            ICityFactory cityFactory, IHexGrid grid, IUnitTerrainCostLogic terrainCostLogic,
            ITilePathDrawer pathDrawer, CitySignals citySignals, UnitSignals unitSignals,
            HexCellSignals cellSignals,
            [Inject(Id = "Combat Summary Display")] CombatSummaryDisplay combatSummaryDisplay
        ){
            DisplaysToManage     = displaysToManage;
            Brain                = brain;
            CombatExecuter       = combatExecuter;
            UnitPositionCanon    = unitPositionCanon;
            CityFactory          = cityFactory;
            Grid                 = grid;
            TerrainCostLogic     = terrainCostLogic;
            PathDrawer           = pathDrawer;
            CitySignals          = citySignals;
            UnitSignals          = unitSignals;
            CellSignals          = cellSignals;
            CombatSummaryDisplay = combatSummaryDisplay;
        }

        #region from UnitUIState

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Clear();

            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastUnitClicked;
                display.Refresh();
            }

            SelectedUnit = Brain.LastUnitClicked;
            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.ReturnViaClick,
                TransitionType.ToUnitSelected, TransitionType.ToCitySelected);

            AttachEvents();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }

            DetachEvents();

            Clear();
        }

        #endregion

        private void OnUnitBeginDrag(Tuple<IUnit, PointerEventData> dataTuple) {
            if(dataTuple.Item1 == SelectedUnit) {
                IsDragging = true;
            }
        }

        private void OnUnitEndDrag(Tuple<IUnit, PointerEventData> dataTuple) {
            if(IsDragging) {
                if(IsCombatValid()) {
                    PerformCombat();
                }else if(IsMovementValid()) {
                    PerformMovement();
                }
            }

            IsDragging = false;
            Clear();
        }

        private void OnCellPointerEntered(IHexCell cell) {
            if(!IsDragging) {
                return;
            }

            IUnit unitOnCell = UnitPositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if( unitOnCell != null && unitOnCell != SelectedUnit &&
                CombatExecuter.CanPerformMeleeAttack(SelectedUnit, unitOnCell)
            ) {
                SetUnitToAttack(unitOnCell);
                return;
            }

            var cityOnCell = CityFactory.AllCities.Where(city => city.Location == cell).FirstOrDefault();

            if(cityOnCell != null && CombatExecuter.CanPerformMeleeAttack(SelectedUnit, cityOnCell)) {
                SetCityToAttack(cityOnCell);
                return;
            }

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(SelectedUnit);

            if(unitLocation != cell && UnitPositionCanon.CanChangeOwnerOfPossession(SelectedUnit, cell)) {
                SetProspectiveTravelGoal(unitLocation, cell);
                return;
            }

            Clear();
        }

        private void OnCellPointerExited(IHexCell cell) {
            if(IsDragging) {
                Clear();
            }
        }

        private void OnUnitPointerEntered(IUnit unit) {
            if(CombatExecuter.CanPerformMeleeAttack(SelectedUnit, unit)) {
                SetUnitToAttack(unit);
            }else {
                Clear();
            }
        }

        private void OnUnitPointerExited(IUnit unit) {
            Clear();
        }

        private void OnCityPointerEntered(ICity city) {
            if(CombatExecuter.CanPerformMeleeAttack(SelectedUnit, city)) {
                SetCityToAttack(city);
            }else {
                Clear();
            }
        }

        private void OnCityPointerExited(ICity city) {
            Clear();
        }

        private void SetUnitToAttack(IUnit unit) {
            Clear();
            UnitToAttack = unit;

            CombatSummaryDisplay.gameObject.SetActive(true);

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(UnitToAttack);
            unitLocation.Overlay.SetDisplayType(CellOverlayType.AttackIndicator);
            unitLocation.Overlay.Show();
        }

        private void SetCityToAttack(ICity city) {
            Clear();
            CityToAttack = city;

            CityToAttack.Location.Overlay.SetDisplayType(CellOverlayType.AttackIndicator);
            CityToAttack.Location.Overlay.Show();
        }

        private void SetProspectiveTravelGoal(IHexCell unitLocation, IHexCell goal) {
            Clear();

            ProspectiveTravelGoal = goal;
            ProspectivePath = Grid.GetShortestPathBetween(
                unitLocation, goal,
                delegate(IHexCell currentCell, IHexCell nextCell) {
                    return TerrainCostLogic.GetTraversalCostForUnit(SelectedUnit, currentCell, nextCell);
                }
            );

            PathDrawer.ClearPath();

            if(ProspectivePath != null) {
                PathDrawer.DrawPath(ProspectivePath);
            }else {
                ProspectiveTravelGoal.Overlay.SetDisplayType(CellOverlayType.UnreachableIndicator);
                ProspectiveTravelGoal.Overlay.Show();
            }
        }

        private bool IsCombatValid() {
            if(UnitToAttack != null && CombatExecuter.CanPerformMeleeAttack(SelectedUnit, UnitToAttack)) {
                return true;
            }else {
                return CityToAttack != null && CombatExecuter.CanPerformMeleeAttack(SelectedUnit, CityToAttack);
            }
        }

        private bool IsMovementValid() {
            return ProspectiveTravelGoal != null && ProspectivePath != null;
        }

        private void PerformCombat() {
            if(CityToAttack != null) {
                CombatExecuter.PerformMeleeAttack(SelectedUnit, CityToAttack);

            }else if(UnitToAttack != null) {
                CombatExecuter.PerformMeleeAttack(SelectedUnit, UnitToAttack);
            }
        }

        private void PerformMovement() {
            SelectedUnit.CurrentPath = ProspectivePath;
            SelectedUnit.PerformMovement();
        }

        private void Clear() {
            if(UnitToAttack != null) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(UnitToAttack);
                unitLocation.Overlay.Clear();
                unitLocation.Overlay.Hide();
            }

            if(CityToAttack != null) {
                CityToAttack.Location.Overlay.Clear();
                CityToAttack.Location.Overlay.Hide();
            }

            if(ProspectiveTravelGoal != null) {
                ProspectiveTravelGoal.Overlay.Clear();
                ProspectiveTravelGoal.Overlay.Hide();
            }

            UnitToAttack = null;
            CityToAttack = null;
            ProspectiveTravelGoal = null;
            ProspectivePath = null;

            PathDrawer.ClearPath();

            CombatSummaryDisplay.gameObject.SetActive(false);
        }

        private void AttachEvents() {
            EventSubscriptions.Add(UnitSignals.BeginDragSignal     .Subscribe(OnUnitBeginDrag));
            EventSubscriptions.Add(UnitSignals.EndDragSignal       .Subscribe(OnUnitEndDrag));
            EventSubscriptions.Add(UnitSignals.PointerEnteredSignal.Subscribe(OnUnitPointerEntered));
            EventSubscriptions.Add(UnitSignals.PointerExitedSignal .Subscribe(OnUnitPointerExited));

            EventSubscriptions.Add(CitySignals.PointerEnteredSignal.Subscribe(OnCityPointerEntered));
            EventSubscriptions.Add(CitySignals.PointerExitedSignal .Subscribe(OnCityPointerExited));

            CellSignals.PointerEnterSignal.Listen(OnCellPointerEntered);
            CellSignals.PointerExitSignal .Listen(OnCellPointerExited);
        }

        private void DetachEvents() {
            foreach(var subscription in EventSubscriptions) {
                subscription.Dispose();
            }
            EventSubscriptions.Clear();

            CellSignals.PointerEnterSignal.Unlisten(OnCellPointerEntered);
            CellSignals.PointerExitSignal .Unlisten(OnCellPointerExited);
        }

        #endregion

    }

}
