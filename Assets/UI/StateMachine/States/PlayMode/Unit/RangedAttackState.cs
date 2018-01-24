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

namespace Assets.UI.StateMachine.States.PlayMode.Unit {

    public class RangedAttackState : StateMachineBehaviour {

        #region instance fields and properties

        private IUnit SelectedUnit;

        private List<IDisposable> EventSubscriptions = new List<IDisposable>();

        private IUnit UnitToAttack {
            get { return _unitToAttack; }
            set {
                if(_unitToAttack != null) {
                    var unitPosition = UnitPositionCanon.GetOwnerOfPossession(_unitToAttack);
                    unitPosition.Overlay.Clear();
                    unitPosition.Overlay.Hide();
                }

                _unitToAttack = value;

                if(_unitToAttack != null) {
                    var unitPosition = UnitPositionCanon.GetOwnerOfPossession(_unitToAttack);
                    unitPosition.Overlay.SetDisplayType(HexMap.CellOverlayType.AttackIndicator);
                    unitPosition.Overlay.Show();
                }
            }
        }
        private IUnit _unitToAttack;

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

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UnitSignals unitSignals, HexCellSignals cellSignals, CitySignals citySignals,
            IUnitPositionCanon unitPositionCanon, ICombatExecuter combatExecuter,
            UIStateMachineBrain brain, List<UnitDisplayBase> displaysToManage,
            [Inject(Id = "Combat Summary Display")] CombatSummaryDisplay combatSummaryDisplay
        ){
            UnitSignals          = unitSignals;
            CellSignals          = cellSignals;
            CitySignals          = citySignals;
            UnitPositionCanon    = unitPositionCanon;
            CombatExecuter       = combatExecuter;
            Brain                = brain;
            DisplaysToManage     = displaysToManage;
            CombatSummaryDisplay = combatSummaryDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            SelectedUnit = Brain.LastUnitClicked;

            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastUnitClicked;
                display.Refresh();
            }

            EventSubscriptions.Add(UnitSignals.PointerEnteredSignal.Subscribe(OnUnitPointerEntered));
            EventSubscriptions.Add(UnitSignals.PointerExitedSignal .Subscribe(OnUnitPointerExited));
            EventSubscriptions.Add(UnitSignals.ClickedSignal       .Subscribe(OnUnitClicked));

            CellSignals.PointerEnterSignal.Listen(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Listen(OnCellPointerExit);
            CellSignals.ClickedSignal     .Listen(OnCellClicked);

            EventSubscriptions.Add(CitySignals.PointerEnteredSignal.Subscribe(OnCityPointerEntered));
            EventSubscriptions.Add(CitySignals.PointerExitedSignal .Subscribe(OnCityPointerExited));
            EventSubscriptions.Add(CitySignals.PointerClickedSignal.Subscribe(OnCityPointerClicked));

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.ReturnViaClick);
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

            foreach(var subscription in EventSubscriptions) {
                subscription.Dispose();
            }
            EventSubscriptions.Clear();

            CellSignals.PointerEnterSignal.Unlisten(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Unlisten(OnCellPointerExit);
            CellSignals.ClickedSignal     .Unlisten(OnCellClicked);

            RequestReturn = false;
        }

        #endregion

        #region Event responses

        private void OnCellPointerEnter(IHexCell cell) {
            UnitToAttack = null;
            CityToAttack = null;

            var unitsOnTile = UnitPositionCanon.GetPossessionsOfOwner(cell);

            if(unitsOnTile.Count() > 0) {
                var attackCandidate = GetUnitToAttackFromStack(unitsOnTile);
                if(attackCandidate != null && CombatExecuter.CanPerformRangedAttack(SelectedUnit, attackCandidate)) {
                    UnitToAttack = attackCandidate;
                }
            }

            RefreshSummaryPanel();
        }

        private void OnCellPointerExit(IHexCell cell) {
            UnitToAttack = null;
            CityToAttack = null;

            RefreshSummaryPanel();
        }

        private void OnCellClicked(IHexCell cell, Vector3 location) {
            if(UnitToAttack != null) {
                CombatExecuter.PerformRangedAttack(SelectedUnit, UnitToAttack);
            }

            if(CityToAttack != null) {
                CombatExecuter.PerformRangedAttack(SelectedUnit, CityToAttack);
            }

            RequestReturn = true;
        }

        private void OnUnitPointerEntered(IUnit unit) {
            CityToAttack = null;

            if(CombatExecuter.CanPerformRangedAttack(SelectedUnit, unit)) {
                UnitToAttack = unit;
            }else {
                UnitToAttack = null;
            }

            RefreshSummaryPanel();
        }

        private void OnUnitPointerExited(IUnit unit) {
            UnitToAttack = null;
            CityToAttack = null;

            RefreshSummaryPanel();
        }

        private void OnUnitClicked(IUnit unit) {
            if(UnitToAttack == null) {
                return;
            }

            CombatExecuter.PerformRangedAttack(SelectedUnit, UnitToAttack);

            RequestReturn = true;
        }

        private void OnCityPointerEntered(ICity city) {
            if(CombatExecuter.CanPerformRangedAttack(SelectedUnit, city)) {
                CityToAttack = city;
            }

            UnitToAttack = null;

            RefreshSummaryPanel();
        }

        private void OnCityPointerExited(ICity city) {
            UnitToAttack = null;
            CityToAttack = null;

            RefreshSummaryPanel();
        }

        private void OnCityPointerClicked(ICity city) {
            if(CityToAttack == null) {
                return;
            }

            CombatExecuter.PerformRangedAttack(SelectedUnit, CityToAttack);

            RequestReturn = true;
        }

        #endregion

        private void RefreshSummaryPanel() {
            if(UnitToAttack != null) {
                CombatSummaryDisplay.AttackingUnit = SelectedUnit;
                CombatSummaryDisplay.DefendingUnit = UnitToAttack;
                CombatSummaryDisplay.gameObject.SetActive(true);

            }else if(CityToAttack != null) {
                CombatSummaryDisplay.AttackingUnit = SelectedUnit;
                CombatSummaryDisplay.DefendingUnit = CityToAttack.CombatFacade;
                CombatSummaryDisplay.gameObject.SetActive(true);

            }else {
                CombatSummaryDisplay.gameObject.SetActive(false);
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

        #endregion

    }

}
