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

namespace Assets.UI.StateMachine.States.PlayMode.Unit {

    public class RangedAttackState : StateMachineBehaviour {

        #region instance fields and properties

        private IDisposable UnitPointerEnteredSubscription;
        private IDisposable UnitPointerExitedSubscription;

        private IUnit UnitToAttack;

        private RectTransform CombatSummaryPanel;


        private UnitSignals UnitSignals;

        private HexCellSignals CellSignals;

        private IUnitPositionCanon UnitPositionCanon;

        private ICombatExecuter CombatExecuter;

        UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UnitSignals unitSignals, HexCellSignals cellSignals,
            IUnitPositionCanon unitPositionCanon, ICombatExecuter combatExecuter,
            UIStateMachineBrain brain
        ){
            UnitSignals       = unitSignals;
            CellSignals       = cellSignals;
            UnitPositionCanon = unitPositionCanon;
            CombatExecuter    = combatExecuter;
            Brain             = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            UnitPointerEnteredSubscription = UnitSignals.PointerEnteredSignal.Subscribe(OnUnitPointerEntered);
            UnitPointerExitedSubscription  = UnitSignals.PointerExitedSignal .Subscribe(OnUnitPointerExited);

            CellSignals.PointerEnterSignal.Listen(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Listen(OnCellPointerExit);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.ReturnViaClick);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            UnitPointerEnteredSubscription.Dispose();
            UnitPointerExitedSubscription.Dispose();

            CellSignals.PointerEnterSignal.Unlisten(OnCellPointerEnter);
            CellSignals.PointerExitSignal .Unlisten(OnCellPointerExit);
        }

        #endregion

        private void OnCellPointerEnter(IHexCell cell) {

        }

        private void OnCellPointerExit(IHexCell cell) {

        }

        private void OnUnitPointerEntered(IUnit unit) {

        }

        private void OnUnitPointerExited(IUnit unit) {

        }

        #endregion

    }

}
