using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

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

        private ICityFactory CityFactory;

        private HexCellOverlayManager OverlayManager;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UnitSignals unitSignals, HexCellSignals cellSignals, CitySignals citySignals,
            IUnitPositionCanon unitPositionCanon, ICombatExecuter combatExecuter,
            UIStateMachineBrain brain,
            [Inject(Id = "Ranged Attack State Displays")] List<UnitDisplayBase> displaysToManage,
            [Inject(Id = "Combat Summary Display")] CombatSummaryDisplay combatSummaryDisplay,
            ICityFactory cityFactory, HexCellOverlayManager overlayManager
        ){
            UnitSignals          = unitSignals;
            CellSignals          = cellSignals;
            CitySignals          = citySignals;
            UnitPositionCanon    = unitPositionCanon;
            CombatExecuter       = combatExecuter;
            Brain                = brain;
            DisplaysToManage     = displaysToManage;
            CombatSummaryDisplay = combatSummaryDisplay;
            CityFactory          = cityFactory;
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
            var cityOnTile = CityFactory.AllCities.Where(city => city.Location == cell).FirstOrDefault();

            if(cityOnTile != null && CombatExecuter.CanPerformRangedAttack(SelectedUnit, cityOnTile)) {
                SetCityToAttack(cityOnTile);
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

        private void OnCellClicked(IHexCell cell, Vector3 location) {
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
            if(CombatExecuter.CanPerformRangedAttack(SelectedUnit, city)) {
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
            CombatSummaryDisplay.gameObject.SetActive(true);
            CombatSummaryDisplay.Refresh();
        }

        private void SetCityToAttack(ICity city) {
            Clear();

            CityToAttack = city;

            OverlayManager.ShowOverlayOfCell(city.Location, CellOverlayType.AttackIndicator);

            CombatSummaryDisplay.AttackingUnit = SelectedUnit;
            CombatSummaryDisplay.DefendingUnit = city.CombatFacade;
            CombatSummaryDisplay.gameObject.SetActive(true);
        }

        private bool IsAttackValid() {
            if(UnitToAttack != null && CombatExecuter.CanPerformRangedAttack(SelectedUnit, UnitToAttack)) {
                return true;
            }else {
                return CityToAttack != null && CombatExecuter.CanPerformRangedAttack(SelectedUnit, CityToAttack);
            }
        }

        private void PerformAttack() {
            if(CityToAttack != null) {
                CombatExecuter.PerformRangedAttack(SelectedUnit, CityToAttack);

            }else if(UnitToAttack != null) {
                CombatExecuter.PerformRangedAttack(SelectedUnit, UnitToAttack);
            }
        }

        private IUnit GetUnitToAttackFromStack(IEnumerable<IUnit> stack) {
            var landMilitary  = stack.Where(unit => unit.Type == UnitType.LandMilitary) .FirstOrDefault();
            var waterMilitary = stack.Where(unit => unit.Type == UnitType.WaterMilitary).FirstOrDefault();

            if(landMilitary != null) {
                return landMilitary;
            }else if(waterMilitary != null) {
                return waterMilitary;
            }else {
                return stack.First();
            }
        }

        private void AttachEvents() {
            EventSubscriptions.Add(UnitSignals.PointerEnteredSignal.Subscribe(OnUnitPointerEntered));
            EventSubscriptions.Add(UnitSignals.PointerExitedSignal .Subscribe(OnUnitPointerExited));
            EventSubscriptions.Add(UnitSignals.ClickedSignal       .Subscribe(OnUnitClicked));

            CellSignals.PointerEnterSignal.Listen(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Listen(OnCellPointerExit);
            CellSignals.ClickedSignal     .Listen(OnCellClicked);

            EventSubscriptions.Add(CitySignals.PointerEnteredSignal.Subscribe(OnCityPointerEntered));
            EventSubscriptions.Add(CitySignals.PointerExitedSignal .Subscribe(OnCityPointerExited));
            EventSubscriptions.Add(CitySignals.PointerClickedSignal.Subscribe(OnCityPointerClicked));
        }

        private void DetachEvents() {
            foreach(var subscription in EventSubscriptions) {
                subscription.Dispose();
            }
            EventSubscriptions.Clear();

            CellSignals.PointerEnterSignal.Unlisten(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Unlisten(OnCellPointerExit);
            CellSignals.ClickedSignal     .Unlisten(OnCellClicked);
        }

        #endregion

    }

}
