using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation;
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

        private IHexCell SelectedUnitPosition {
            get { return UnitPositionCanon.GetOwnerOfPossession(SelectedUnit); }
        }

        private IUnit UnitToAttack;

        private ICity CityToAttack;

        private IHexCell ProspectiveTravelGoal;

        private List<IHexCell> ProspectivePath;

        private bool IsDragging;

        private List<IDisposable> EventSubscriptions = new List<IDisposable>();




        private List<UnitDisplayBase>                    DisplaysToManage;
        private UIStateMachineBrain                      Brain;
        private ICombatExecuter                          CombatExecuter;
        private IUnitPositionCanon                       UnitPositionCanon;
        private IHexGrid                                 Grid;
        private IUnitTerrainCostLogic                    TerrainCostLogic;
        private ICellPathDrawer                          PathDrawer;
        private CitySignals                              CitySignals;
        private UnitSignals                              UnitSignals;
        private HexCellSignals                           CellSignals;
        private HexCellOverlayManager                    OverlayManager;
        private CombatSummaryDisplay                     CombatSummaryDisplay;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            List<UnitDisplayBase> displaysToManage,
            UIStateMachineBrain brain,
            ICombatExecuter combatExecuter,
            IUnitPositionCanon unitPositionCanon,
            IHexGrid grid,
            IUnitTerrainCostLogic terrainCostLogic,
            ICellPathDrawer pathDrawer,
            CitySignals citySignals,
            UnitSignals unitSignals,
            HexCellSignals cellSignals,
            HexCellOverlayManager overlayManager,
            [Inject(Id = "Combat Summary Display")] CombatSummaryDisplay combatSummaryDisplay,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            DisplaysToManage     = displaysToManage;
            Brain                = brain;
            CombatExecuter       = combatExecuter;
            UnitPositionCanon    = unitPositionCanon;
            Grid                 = grid;
            TerrainCostLogic     = terrainCostLogic;
            PathDrawer           = pathDrawer;
            CitySignals          = citySignals;
            UnitSignals          = unitSignals;
            CellSignals          = cellSignals;
            OverlayManager       = overlayManager;
            CombatSummaryDisplay = combatSummaryDisplay;
            CityLocationCanon    = cityLocationCanon;
        }

        #region from UnitUIState

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastUnitClicked;
                display.Refresh();
            }

            SelectedUnit = Brain.LastUnitClicked;
            Brain.ClearListeners();

            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.ReturnViaClick,
                TransitionType.UnitSelected, TransitionType.CitySelected);
            Brain.EnableCameraMovement();
            Brain.EnableCellHovering();

            AttachEvents();

            Clear();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if(SelectedUnit == null) {
                animator.SetTrigger("Return Requested");
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }

            DetachEvents();

            SelectedUnit = null;

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

            if(unitOnCell == SelectedUnit) {
                return;
            }

            if(unitOnCell != null && CombatExecuter.CanPerformMeleeAttack(SelectedUnit, unitOnCell)) {
                SetUnitToAttack(unitOnCell);
                return;
            }

            var cityOnCell = CityLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(cityOnCell != null && CombatExecuter.CanPerformMeleeAttack(SelectedUnit, cityOnCell.CombatFacade)) {
                SetCityToAttack(cityOnCell);
                return;
            }

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(SelectedUnit);

            if(unitLocation != cell && UnitPositionCanon.CanChangeOwnerOfPossession(SelectedUnit, cell)) {
                SetProspectiveTravelGoal(unitLocation, cell);
            }else {
                Clear();
            }
        }

        private void OnCellPointerExited(IHexCell cell) {
            if(IsDragging) {
                Clear();
            }
        }

        private void OnUnitPointerEntered(IUnit unit) {
            if(!IsDragging) {
                return;
            }

            if(CombatExecuter.CanPerformMeleeAttack(SelectedUnit, unit)) {
                SetUnitToAttack(unit);
            }else {
                Clear();
            }
        }

        private void OnUnitPointerExited(IUnit unit) {
            if(IsDragging) {
                Clear();
            }
        }

        private void OnUnitBeingDestroyed(IUnit unitBeingDestroyed) {
            if(unitBeingDestroyed == SelectedUnit) {
                SelectedUnit = null;
            }
        }

        private void OnCityPointerEntered(ICity city) {
            if(!IsDragging) {
                return;
            }

            if(CombatExecuter.CanPerformMeleeAttack(SelectedUnit, city.CombatFacade)) {
                SetCityToAttack(city);
                return;
            }

            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            if(UnitPositionCanon.CanPlaceUnitAtLocation(SelectedUnit, cityLocation, false)) {
                var selectedUnitLocation = UnitPositionCanon.GetOwnerOfPossession(SelectedUnit);
                SetProspectiveTravelGoal(selectedUnitLocation, cityLocation);

            } else {
                Clear();
            }
        }

        private void OnCityPointerExited(ICity city) {
            if(IsDragging) {
                Clear();
            }
        }

        private void SetUnitToAttack(IUnit unit) {
            Clear();
            UnitToAttack = unit;

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(UnitToAttack);
            OverlayManager.ShowOverlayOfCell(unitLocation, CellOverlayType.AttackIndicator);

            CombatSummaryDisplay.AttackingUnit = SelectedUnit;
            CombatSummaryDisplay.DefendingUnit = unit;
            CombatSummaryDisplay.IsMeleeAttack = true;

            CombatSummaryDisplay.gameObject.SetActive(true);

            CombatSummaryDisplay.Refresh();
        }

        private void SetCityToAttack(ICity city) {
            Clear();
            CityToAttack = city;
            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            OverlayManager.ShowOverlayOfCell(cityLocation, CellOverlayType.AttackIndicator);

            CombatSummaryDisplay.AttackingUnit = SelectedUnit;
            CombatSummaryDisplay.DefendingUnit = city.CombatFacade;
            CombatSummaryDisplay.IsMeleeAttack = true;

            CombatSummaryDisplay.gameObject.SetActive(true);

            CombatSummaryDisplay.Refresh();
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
                OverlayManager.ShowOverlayOfCell(ProspectiveTravelGoal, CellOverlayType.UnreachableIndicator);
            }
        }

        private bool IsCombatValid() {
            if(UnitToAttack != null && CombatExecuter.CanPerformMeleeAttack(SelectedUnit, UnitToAttack)) {
                return true;
            }else {
                return CityToAttack != null && CombatExecuter.CanPerformMeleeAttack(SelectedUnit, CityToAttack.CombatFacade);
            }
        }

        private bool IsMovementValid() {
            return ProspectiveTravelGoal != null && ProspectivePath != null;
        }

        private void PerformCombat() {
            if(CityToAttack != null) {
                CombatExecuter.PerformMeleeAttack(SelectedUnit, CityToAttack.CombatFacade);

            }else if(UnitToAttack != null) {
                CombatExecuter.PerformMeleeAttack(SelectedUnit, UnitToAttack);
            }
        }

        private void PerformMovement() {
            SelectedUnit.CurrentPath = ProspectivePath;
            SelectedUnit.PerformMovement();
        }

        private void Clear() {
            OverlayManager.ClearAllOverlays();
            if(SelectedUnit != null && SelectedUnitPosition != null) {
                OverlayManager.ShowOverlayOfCell(SelectedUnitPosition, CellOverlayType.SelectedIndicator);
            }

            UnitToAttack = null;
            CityToAttack = null;
            ProspectiveTravelGoal = null;
            ProspectivePath = null;

            PathDrawer.ClearPath();

            CombatSummaryDisplay.gameObject.SetActive(false);
        }

        private void AttachEvents() {
            EventSubscriptions.Add(UnitSignals.BeginDragSignal         .Subscribe(OnUnitBeginDrag));
            EventSubscriptions.Add(UnitSignals.EndDragSignal           .Subscribe(OnUnitEndDrag));
            EventSubscriptions.Add(UnitSignals.PointerEnteredSignal    .Subscribe(OnUnitPointerEntered));
            EventSubscriptions.Add(UnitSignals.PointerExitedSignal     .Subscribe(OnUnitPointerExited));
            EventSubscriptions.Add(UnitSignals.UnitBeingDestroyedSignal.Subscribe(OnUnitBeingDestroyed));

            EventSubscriptions.Add(CitySignals.PointerEnteredSignal.Subscribe(OnCityPointerEntered));
            EventSubscriptions.Add(CitySignals.PointerExitedSignal .Subscribe(OnCityPointerExited));
            EventSubscriptions.Add(CellSignals.PointerEnterSignal  .Subscribe(OnCellPointerEntered));
            EventSubscriptions.Add(CellSignals.PointerExitSignal   .Subscribe(OnCellPointerExited));
        }

        private void DetachEvents() {
            foreach(var subscription in EventSubscriptions) {
                subscription.Dispose();
            }
            EventSubscriptions.Clear();
        }

        #endregion

    }

}
