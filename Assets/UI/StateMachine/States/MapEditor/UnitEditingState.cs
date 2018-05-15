using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.UI.MapEditor;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class UnitEditingState : StateMachineBehaviour {

        #region instance fields and properties

        private IUnit SelectedUnit;

        private IHexCell ProspectiveNewLocation;

        private bool IsDragging;

        private List<IDisposable> EventSubscriptions = new List<IDisposable>();



        private UnitEditingPanel    UnitEditingPanel;
        private UIStateMachineBrain Brain;
        private IUnitPositionCanon  UnitPositionCanon;
        private UnitSignals         UnitSignals;
        private HexCellSignals      CellSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UnitEditingPanel unitEditingPanel, UIStateMachineBrain brain,
            IUnitPositionCanon unitPositionCanon, UnitSignals unitSignals,
            HexCellSignals cellSignals
        ){
            UnitEditingPanel  = unitEditingPanel;
            Brain             = brain;
            UnitPositionCanon = unitPositionCanon;
            UnitSignals       = unitSignals;
            CellSignals       = cellSignals;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            UnitEditingPanel.UnitToEdit = Brain.LastUnitClicked;

            UnitEditingPanel.gameObject.SetActive(true);

            Brain.ClearListeners();

            Brain.EnableCameraMovement();
            Brain.DisableCellHovering();

            Brain.ListenForTransitions(
                TransitionType.ReturnViaButton, TransitionType.ReturnViaClick, TransitionType.UnitSelected
            );

            AttachEvents();

            SelectedUnit = Brain.LastUnitClicked;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            UnitEditingPanel.gameObject.SetActive(false);

            DetachEvents();

            SelectedUnit           = null;
            ProspectiveNewLocation = null;
            IsDragging             = false;
        }

        #endregion

        private void AttachEvents() {
            EventSubscriptions.Add(UnitSignals.BeginDragSignal.Subscribe(OnUnitBeginDrag));
            EventSubscriptions.Add(UnitSignals.EndDragSignal  .Subscribe(OnUnitEndDrag));

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

        private void OnUnitBeginDrag(Tuple<IUnit, PointerEventData> data) {
            if(data.Item1 == SelectedUnit) {
                IsDragging = true;
            }
        }

        private void OnUnitEndDrag(Tuple<IUnit, PointerEventData> data) {
            if(IsDragging && IsLocationValid()) {
                SelectedUnit.Relocate(ProspectiveNewLocation);
            }

            IsDragging = false;

            ProspectiveNewLocation = null;
        }

        private void OnCellPointerEntered(IHexCell cell) {
            if(!IsDragging) {
                return;
            }

            if(cell != null && UnitPositionCanon.CanChangeOwnerOfPossession(SelectedUnit, cell)) {
                ProspectiveNewLocation = cell;
            }else {
                ProspectiveNewLocation = null;
            }
        }

        private void OnCellPointerExited(IHexCell cell) {
            if(IsDragging) {
                ProspectiveNewLocation = null;
            }
        }

        private bool IsLocationValid() {
            return ProspectiveNewLocation != null 
                && UnitPositionCanon.CanChangeOwnerOfPossession(SelectedUnit, ProspectiveNewLocation);
        }

        #endregion

    }

}
