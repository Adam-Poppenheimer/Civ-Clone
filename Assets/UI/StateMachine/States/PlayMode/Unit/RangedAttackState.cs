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
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

using Assets.UI.Units;
using Assets.UI.HexMap;

namespace Assets.UI.StateMachine.States.PlayMode.Unit {

    public class RangedAttackState : StateMachineBehaviour {

        #region instance fields and properties

        private IUnit SelectedUnit;

        private IHexCell SelectedUnitPosition {
            get { return UnitPositionCanon.GetOwnerOfPossession(SelectedUnit); }
        }

        private List<IDisposable> EventSubscriptions = new List<IDisposable>();

        private IUnit UnitToAttack;

        private ICity CityToAttack;

        private bool RequestReturn;



        private UnitSignals UnitSignals;

        private HexCellSignals CellSignals;

        private CitySignals CitySignals;

        private IUnitPositionCanon UnitPositionCanon;

        private ICombatExecuter CombatExecuter;

        UIStateMachineBrain Brain;

        private List<UnitDisplayBase> DisplaysToManage;

        private CombatSummaryDisplay CombatSummaryDisplay;

        private IHexCellOverlayManager OverlayManager;

        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UnitSignals unitSignals, HexCellSignals cellSignals, CitySignals citySignals,
            IUnitPositionCanon unitPositionCanon, ICombatExecuter combatExecuter,
            UIStateMachineBrain brain, IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            [Inject(Id = "Ranged Attack State Displays")] List<UnitDisplayBase> displaysToManage,
            [Inject(Id = "Combat Summary Display")] CombatSummaryDisplay combatSummaryDisplay,
            IHexCellOverlayManager overlayManager
        ){
            UnitSignals          = unitSignals;
            CellSignals          = cellSignals;
            CitySignals          = citySignals;
            UnitPositionCanon    = unitPositionCanon;
            CombatExecuter       = combatExecuter;
            Brain                = brain;
            CityLocationCanon    = cityLocationCanon;
            DisplaysToManage     = displaysToManage;
            CombatSummaryDisplay = combatSummaryDisplay;
            OverlayManager       = overlayManager;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            SelectedUnit = Brain.LastUnitClicked;

            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastUnitClicked;
                display.Refresh();
            }

            Brain.ClearListeners();

            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.ReturnViaClick);
            Brain.EnableCameraMovement();

            AttachEvents();

            Clear();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if(RequestReturn) {
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

            RequestReturn = false;

            Clear();
        }

        #endregion

        #region Event responses

        private void OnCellPointerEnter(IHexCell cell) {
            var cityOnCell = CityLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(cityOnCell != null && CombatExecuter.CanPerformRangedAttack(SelectedUnit, cityOnCell.CombatFacade)) {
                SetCityToAttack(cityOnCell);
                return;
            }

            var unitsOnTile = UnitPositionCanon.GetPossessionsOfOwner(cell).Where(unit => unit.Type != UnitType.City);

            if(unitsOnTile.Count() > 0) {
                var attackCandidate = GetUnitToAttackFromStack(unitsOnTile);
                if(attackCandidate != null && CombatExecuter.CanPerformRangedAttack(SelectedUnit, attackCandidate)) {
                    SetUnitToAttack(attackCandidate);
                    return;
                }
            }

            Clear();
        }

        private void OnCellPointerExit(IHexCell cell) {
            Clear();
        }

        private void OnCellClicked(Tuple<IHexCell, PointerEventData> data) {
            if(IsAttackValid()) {
                PerformAttack();
            }

            Clear();

            RequestReturn = true;
        }

        private void OnUnitPointerEntered(IUnit unit) {
            if(CombatExecuter.CanPerformRangedAttack(SelectedUnit, unit)) {
                SetUnitToAttack(unit);
            }else {
                Clear();
            }
        }

        private void OnUnitPointerExited(IUnit unit) {
            Clear();
        }

        private void OnUnitClicked(IUnit unit) {
            if(IsAttackValid()) {
                PerformAttack();
            }

            Clear();

            RequestReturn = true;
        }

        private void OnCityPointerEntered(ICity city) {
            if(CombatExecuter.CanPerformRangedAttack(SelectedUnit, city.CombatFacade)) {
                SetCityToAttack(city);
            }else {
                Clear();
            }
        }

        private void OnCityPointerExited(ICity city) {
            Clear();
        }

        private void OnCityPointerClicked(ICity city) {
            if(IsAttackValid()) {
                PerformAttack();
            }

            Clear();

            RequestReturn = true;
        }

        #endregion

        private void Clear() {
            OverlayManager.ClearAllOverlays();
            if(SelectedUnit != null) {
                OverlayManager.ShowOverlayOfCell(SelectedUnitPosition, CellOverlayType.SelectedIndicator);
            }

            UnitToAttack = null;
            CityToAttack = null;

            CombatSummaryDisplay.gameObject.SetActive(false);
        }

        private void SetUnitToAttack(IUnit unit) {
            Clear();

            UnitToAttack = unit;

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            OverlayManager.ShowOverlayOfCell(unitLocation, CellOverlayType.AttackIndicator);

            CombatSummaryDisplay.AttackingUnit = SelectedUnit;
            CombatSummaryDisplay.DefendingUnit = UnitToAttack;
            CombatSummaryDisplay.IsMeleeAttack = false;

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
            CombatSummaryDisplay.IsMeleeAttack = false;

            CombatSummaryDisplay.gameObject.SetActive(true);

            CombatSummaryDisplay.Refresh();
        }

        private bool IsAttackValid() {
            if(UnitToAttack != null && CombatExecuter.CanPerformRangedAttack(SelectedUnit, UnitToAttack)) {
                return true;
            }else {
                return CityToAttack != null && CombatExecuter.CanPerformRangedAttack(SelectedUnit, CityToAttack.CombatFacade);
            }
        }

        private void PerformAttack() {
            if(CityToAttack != null) {
                CombatExecuter.PerformRangedAttack(SelectedUnit, CityToAttack.CombatFacade);

            }else if(UnitToAttack != null) {
                CombatExecuter.PerformRangedAttack(SelectedUnit, UnitToAttack);
            }
        }

        private IUnit GetUnitToAttackFromStack(IEnumerable<IUnit> stack) {
            var landMilitary  = stack.Where(unit => unit.Type.IsLandMilitary()) .FirstOrDefault();
            var waterMilitary = stack.Where(unit => unit.Type.IsWaterMilitary()).FirstOrDefault();

            if(landMilitary != null) {
                return landMilitary;
            }else if(waterMilitary != null) {
                return waterMilitary;
            }else {
                return stack.First();
            }
        }

        private void AttachEvents() {
            EventSubscriptions.Add(UnitSignals.PointerEntered.Subscribe(OnUnitPointerEntered));
            EventSubscriptions.Add(UnitSignals.PointerExited .Subscribe(OnUnitPointerExited));
            EventSubscriptions.Add(UnitSignals.Clicked       .Subscribe(OnUnitClicked));

            EventSubscriptions.Add(CellSignals.PointerEnterSignal.Subscribe(OnCellPointerEnter));
            EventSubscriptions.Add(CellSignals.PointerExitSignal .Subscribe(OnCellPointerExit));
            EventSubscriptions.Add(CellSignals.ClickedSignal     .Subscribe(OnCellClicked));

            EventSubscriptions.Add(CitySignals.PointerEnteredSignal.Subscribe(OnCityPointerEntered));
            EventSubscriptions.Add(CitySignals.PointerExitedSignal .Subscribe(OnCityPointerExited));
            EventSubscriptions.Add(CitySignals.PointerClickedSignal.Subscribe(OnCityPointerClicked));
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
