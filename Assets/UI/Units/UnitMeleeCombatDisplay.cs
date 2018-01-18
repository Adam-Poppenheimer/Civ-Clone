using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.HexMap;

namespace Assets.UI.Units {

    public class UnitMeleeCombatDisplay : UnitDisplayBase {

        #region instance fields and properties

        private IDisposable UnitBeginDragSubscription;
        private IDisposable UnitDragSubscription;
        private IDisposable UnitEndDragSubscription;
        private IDisposable UnitPointerEnteredSubscription;
        private IDisposable UnitPointerExitedSubscription;

        private bool SearchForPossibleAttacks;

        private IUnit UnitToAttack;

        [SerializeField] private RectTransform CombatSummaryPanel;



        private UnitSignals UnitSignals;

        private HexCellSignals CellSignals;

        private IUnitPositionCanon UnitPositionCanon;

        private ICombatExecuter CombatExecuter;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UnitSignals unitSignals, HexCellSignals cellSignals,
            IUnitPositionCanon unitPositionCanon, ICombatExecuter combatExecuter
        ){
            UnitSignals        = unitSignals;
            CellSignals       = cellSignals;
            UnitPositionCanon = unitPositionCanon;
            CombatExecuter    = combatExecuter;
        }

        #region from UnitDisplayBase

        protected override void DoOnEnable() {
            UnitBeginDragSubscription      = UnitSignals.BeginDragSignal     .Subscribe(OnUnitBeginDrag);
            UnitDragSubscription           = UnitSignals.DragSignal          .Subscribe(OnUnitDrag);
            UnitEndDragSubscription        = UnitSignals.EndDragSignal       .Subscribe(OnUnitEndDrag);
            UnitPointerEnteredSubscription = UnitSignals.PointerEnteredSignal.Subscribe(OnUnitPointerEntered);
            UnitPointerExitedSubscription  = UnitSignals.PointerExitedSignal .Subscribe(OnUnitPointerExited);

            CellSignals.PointerEnterSignal.Listen(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Listen(OnCellPointerExit);
        }

        protected override void DoOnDisable() {
            UnitBeginDragSubscription     .Dispose();
            UnitDragSubscription          .Dispose();
            UnitEndDragSubscription       .Dispose();
            UnitPointerEnteredSubscription.Dispose();
            UnitPointerExitedSubscription .Dispose();

            CellSignals.PointerEnterSignal.Unlisten(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Unlisten(OnCellPointerExit);

            SearchForPossibleAttacks = false;

            UnitToAttack = null;

            CombatSummaryPanel.gameObject.SetActive(false);
        }

        #endregion

        private void OnUnitBeginDrag(Tuple<IUnit, PointerEventData> tuple) {
            SearchForPossibleAttacks = tuple.Item1 == ObjectToDisplay;
        }

        private void OnUnitDrag(Tuple<IUnit, PointerEventData> data) {
            if(UnitToAttack != null) {
                CombatSummaryPanel.gameObject.SetActive(true);
                CombatSummaryPanel.position = Camera.main.WorldToScreenPoint(UnitToAttack.gameObject.transform.position);
            }else {
                CombatSummaryPanel.gameObject.SetActive(false);
            }
        }

        private void OnUnitEndDrag(Tuple<IUnit, PointerEventData> tuple) {
            if(UnitToAttack != null) {
                CombatExecuter.PerformMeleeAttack(ObjectToDisplay, UnitToAttack);
            }

            CombatSummaryPanel.gameObject.SetActive(false);

            SearchForPossibleAttacks = false;
        }

        private void OnUnitPointerEntered(IUnit unit) {
            if(!SearchForPossibleAttacks) {
                return;
            }

            if(CombatExecuter.CanPerformMeleeAttack(ObjectToDisplay, unit)) {
                UnitToAttack = unit;
            }else {
                UnitToAttack = null;
            }
        }

        private void OnUnitPointerExited(IUnit unit) {
            UnitToAttack = null;
        }

        private void OnCellPointerEnter(IHexCell cell) {
            if(!SearchForPossibleAttacks) {
                return;
            }

            var unitOnCell = UnitPositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if( unitOnCell != null && unitOnCell != ObjectToDisplay &&
                CombatExecuter.CanPerformMeleeAttack(ObjectToDisplay, unitOnCell)
            ){
                UnitToAttack = unitOnCell;
            }else {
                UnitToAttack = null;
            }
        }

        private void OnCellPointerExit(IHexCell cell) {
            UnitToAttack = null;
        }

        #endregion

    }

}
