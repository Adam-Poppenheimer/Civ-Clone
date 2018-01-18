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

using Assets.UI.Units;

namespace Assets.UI.StateMachine.States.PlayMode.Unit {

    public class RangedAttackState : StateMachineBehaviour {

        #region instance fields and properties

        private IUnit SelectedUnit;

        private IDisposable UnitPointerEnteredSubscription;
        private IDisposable UnitPointerExitedSubscription;
        private IDisposable UnitPointerClickedSubscription;

        private IUnit UnitToAttack;

        private RectTransform CombatSummaryPanel;

        private bool RequestReturn;



        private UnitSignals UnitSignals;

        private HexCellSignals CellSignals;

        private IUnitPositionCanon UnitPositionCanon;

        private ICombatExecuter CombatExecuter;

        UIStateMachineBrain Brain;

        private List<UnitDisplayBase> DisplaysToManage;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UnitSignals unitSignals, HexCellSignals cellSignals,
            IUnitPositionCanon unitPositionCanon, ICombatExecuter combatExecuter,
            UIStateMachineBrain brain, List<UnitDisplayBase> displaysToManage,
            [Inject(Id = "Combat Summary Panel")] RectTransform combatSummaryPanel
        ){
            UnitSignals        = unitSignals;
            CellSignals        = cellSignals;
            UnitPositionCanon  = unitPositionCanon;
            CombatExecuter     = combatExecuter;
            Brain              = brain;
            DisplaysToManage   = displaysToManage;
            CombatSummaryPanel = combatSummaryPanel;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            SelectedUnit = Brain.LastUnitClicked;

            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastUnitClicked;
                display.Refresh();
            }

            UnitPointerEnteredSubscription = UnitSignals.PointerEnteredSignal.Subscribe(OnUnitPointerEntered);
            UnitPointerExitedSubscription  = UnitSignals.PointerExitedSignal .Subscribe(OnUnitPointerExited);
            UnitPointerClickedSubscription = UnitSignals.ClickedSignal       .Subscribe(OnUnitClicked);

            CellSignals.PointerEnterSignal.Listen(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Listen(OnCellPointerExit);
            CellSignals.ClickedSignal     .Listen(OnCellClicked);

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

            UnitPointerEnteredSubscription.Dispose();
            UnitPointerExitedSubscription .Dispose();
            UnitPointerClickedSubscription.Dispose();

            CellSignals.PointerEnterSignal.Unlisten(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Unlisten(OnCellPointerExit);
            CellSignals.ClickedSignal     .Unlisten(OnCellClicked);

            RequestReturn = false;
        }

        #endregion

        private void OnCellPointerEnter(IHexCell cell) {
            UnitToAttack = null;

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

            RefreshSummaryPanel();
        }

        private void OnUnitPointerEntered(IUnit unit) {
            if(CombatExecuter.CanPerformRangedAttack(SelectedUnit, unit)) {
                UnitToAttack = unit;
            }else {
                UnitToAttack = null;
            }

            RefreshSummaryPanel();
        }

        private void OnUnitPointerExited(IUnit unit) {
            UnitToAttack = null;

            RefreshSummaryPanel();
        }

        private void RefreshSummaryPanel() {
            if(UnitToAttack != null) {
                CombatSummaryPanel.gameObject.SetActive(true);
                CombatSummaryPanel.transform.position = Camera.main.WorldToScreenPoint(UnitToAttack.gameObject.transform.position);
            }else {
                CombatSummaryPanel.gameObject.SetActive(false);
            }  
        }

        private IUnit GetUnitToAttackFromStack(IEnumerable<IUnit> stack) {
            var landMilitary  = stack.Where(unit => unit.Template.Type == UnitType.LandMilitary) .FirstOrDefault();
            var waterMilitary = stack.Where(unit => unit.Template.Type == UnitType.WaterMilitary).FirstOrDefault();

            if(landMilitary != null) {
                return landMilitary;
            }else if(waterMilitary != null) {
                return waterMilitary;
            }else {
                return stack.First();
            }
        }

        private void OnUnitClicked(IUnit unit) {
            if(UnitToAttack != null) {
                CombatExecuter.PerformRangedAttack(SelectedUnit, UnitToAttack);
            }

            RequestReturn = true;
        }

        private void OnCellClicked(IHexCell cell, Vector3 location) {
            if(UnitToAttack != null) {
                CombatExecuter.PerformRangedAttack(SelectedUnit, UnitToAttack);
            }

            RequestReturn = true;
        }

        #endregion

    }

}
