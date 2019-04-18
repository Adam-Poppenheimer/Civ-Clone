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
        private IUnitAttackOrderLogic                    AttackOrderLogic;
        private IUnitPositionCanon                       UnitPositionCanon;
        private IHexPathfinder                           HexPathfinder;
        private ICellPathDrawer                          PathDrawer;
        private CitySignals                              CitySignals;
        private UnitSignals                              UnitSignals;
        private HexCellSignals                           CellSignals;
        private IHexCellOverlayManager                   OverlayManager;
        private CombatSummaryDisplay                     CombatSummaryDisplay;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private IGameCamera                              GameCamera;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            List<UnitDisplayBase> displaysToManage,
            UIStateMachineBrain brain,
            ICombatExecuter combatExecuter,
            IUnitAttackOrderLogic attackOrderLogic,
            IUnitPositionCanon unitPositionCanon,
            IHexPathfinder hexPathfinder,
            ICellPathDrawer pathDrawer,
            CitySignals citySignals,
            UnitSignals unitSignals,
            HexCellSignals cellSignals,
            IHexCellOverlayManager overlayManager,
            [Inject(Id = "Combat Summary Display")] CombatSummaryDisplay combatSummaryDisplay,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            DisplaysToManage     = displaysToManage;
            Brain                = brain;
            CombatExecuter       = combatExecuter;
            AttackOrderLogic     = attackOrderLogic;
            UnitPositionCanon    = unitPositionCanon;
            HexPathfinder        = hexPathfinder;
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

            Brain.ListenForTransitions(
                TransitionType.ReturnViaButton,       TransitionType.ReturnViaClick,
                TransitionType.ActiveCivUnitSelected, TransitionType.CitySelected
            );
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
            ProspectivePath = HexPathfinder.GetShortestPathBetween(
                unitLocation, goal,
                delegate(IHexCell currentCell, IHexCell nextCell) {
                    return UnitPositionCanon.GetTraversalCostForUnit(
                        SelectedUnit, currentCell, nextCell, false
                    );
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
                CombatExecuter.PerformMeleeAttack(SelectedUnit, CityToAttack.CombatFacade, () => { }, () => { });

            }else if(UnitToAttack != null) {
                CombatExecuter.PerformMeleeAttack(SelectedUnit, UnitToAttack, () => { }, () => { });
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
            EventSubscriptions.Add(UnitSignals.BeginDrag         .Subscribe(OnUnitBeginDrag));
            EventSubscriptions.Add(UnitSignals.EndDrag           .Subscribe(OnUnitEndDrag));
            EventSubscriptions.Add(UnitSignals.PointerEntered    .Subscribe(OnUnitPointerEntered));
            EventSubscriptions.Add(UnitSignals.PointerExited     .Subscribe(OnUnitPointerExited));
            EventSubscriptions.Add(UnitSignals.BeingDestroyed.Subscribe(OnUnitBeingDestroyed));
            EventSubscriptions.Add(UnitSignals.EnteredLocation   .Subscribe(OnUnitEnteredLocation));

            EventSubscriptions.Add(CitySignals.PointerEntered.Subscribe(OnCityPointerEntered));
            EventSubscriptions.Add(CitySignals.PointerExited .Subscribe(OnCityPointerExited));
            EventSubscriptions.Add(CellSignals.PointerEnter  .Subscribe(OnCellPointerEntered));
            EventSubscriptions.Add(CellSignals.PointerExit   .Subscribe(OnCellPointerExited));
        }

        private void DetachEvents() {
            EventSubscriptions.ForEach(subscription => subscription.Dispose());
            EventSubscriptions.Clear();
        }

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

            IUnit attackCandidate = AttackOrderLogic.GetNextAttackTargetOnCell(cell);

            if(attackCandidate == SelectedUnit) {
                return;
            }

            if(attackCandidate != null && CombatExecuter.CanPerformMeleeAttack(SelectedUnit, attackCandidate)) {
                SetUnitToAttack(attackCandidate);
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

        private void OnUnitEnteredLocation(Tuple<IUnit, IHexCell> data) {
            Clear();
        }

        #endregion

    }

}
