using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.UI.MapEditor {

    public class RiverPaintingPanel : MonoBehaviour {

        #region instance fields and properties

        private bool AddingOrRemoving;

        private bool IsDragging;

        private IHexCell CurrentCell;

        private HexDirection CurrentSextant;

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();
        



        private IRiverCanon    RiverCanon;
        private IHexGrid       Grid;
        private HexCellSignals CellSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IRiverCanon riverCanon, IHexGrid grid, HexCellSignals cellSignals
        ) {
            RiverCanon  = riverCanon;
            Grid        = grid;
            CellSignals = cellSignals;
        }

        #region Unity messages

        private void Update() {
            if(IsDragging && CurrentCell != null && HasMouseMovedThisFrame()) {
                if(AddingOrRemoving) {
                    var nextSextant = GetSextantOfMouse(CurrentCell);

                    if(nextSextant != CurrentSextant) {
                        TryDrawRiverWithinCell(CurrentCell, CurrentSextant, nextSextant);

                        CurrentSextant  = nextSextant;
                    }
                }else {
                    RiverCanon.RemoveRiverFromCellInDirection(CurrentCell, CurrentSextant);
                }
            }
        }

        private void OnEnable() {
            SignalSubscriptions.Add(CellSignals.BeginDragSignal   .Subscribe(OnCellBeginDrag));
            SignalSubscriptions.Add(CellSignals.EndDragSignal     .Subscribe(OnCellEndDrag));
            SignalSubscriptions.Add(CellSignals.PointerEnterSignal.Subscribe(OnCellPointerEnter));
            SignalSubscriptions.Add(CellSignals.PointerExitSignal .Subscribe(OnCellPointerExit));
        }

        private void OnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());

            SignalSubscriptions.Clear();

            CurrentCell = null;
            IsDragging = false;
        }

        #endregion

        public void SetRiverMode(bool addingOrRemoving) {
            AddingOrRemoving = addingOrRemoving;
        }

        private void OnCellBeginDrag(HexCellDragData dragData) {
            CurrentCell    = dragData.CellBeingDragged;
            CurrentSextant = GetSextantOfMouse(CurrentCell);
            IsDragging = true;
        }

        private void OnCellEndDrag(HexCellDragData dragData) {
            CurrentCell  = null;

            IsDragging = false;
        }

        private void OnCellPointerEnter(IHexCell cell) {
            if(!IsDragging) {
                return;
            }

            if(CurrentCell != null && cell != CurrentCell) {
                if(AddingOrRemoving) {
                    TryDrawRiverBetweenCells(CurrentCell, cell, CurrentSextant);
                }else {
                    RiverCanon.RemoveRiverFromCellInDirection(CurrentCell, CurrentSextant);
                }                
            }

            CurrentCell    = cell;
            CurrentSextant = GetSextantOfMouse(cell);
        }

        private void OnCellPointerExit(IHexCell cell) {
            if(!IsDragging) {
                return;
            }

            CurrentCell = null;
        }

        private void TryDrawRiverWithinCell(IHexCell currentCell, HexDirection currentSextant, HexDirection nextSextant) {
            RiverFlow? flowOfRiver = GetFlowFromSextants(currentSextant, nextSextant);

            if( flowOfRiver != null &&
                RiverCanon.CanAddRiverToCell(currentCell, currentSextant, flowOfRiver.GetValueOrDefault())
            ) {
                RiverCanon.AddRiverToCell   (currentCell, currentSextant, flowOfRiver.GetValueOrDefault());
            }
        }

        private void TryDrawRiverBetweenCells(IHexCell currentCell, IHexCell nextCell, HexDirection currentSextant) {
            RiverFlow? flowOfRiver = GetFlowFromEdgeMidpoint(currentCell, currentSextant);

            if( flowOfRiver != null &&
                RiverCanon.CanAddRiverToCell(currentCell, currentSextant, flowOfRiver.GetValueOrDefault())
            ) {
                RiverCanon.AddRiverToCell   (currentCell, currentSextant, flowOfRiver.GetValueOrDefault());
            }
        }

        private HexDirection GetNearestEdgeOfCellFromPoint(IHexCell cell, Vector3 mouseRayHit) {
            var directions = EnumUtil.GetValues<HexDirection>().ToList();

            directions.Sort(delegate(HexDirection directionOne, HexDirection directionTwo) {
                var edgeOneMiddle = cell.Position + HexMetrics.GetOuterEdgeMiddle(directionOne);
                var edgeTwoMiddle = cell.Position + HexMetrics.GetOuterEdgeMiddle(directionTwo);

                return Vector3.Distance(mouseRayHit, edgeOneMiddle)
                    .CompareTo(Vector3.Distance(mouseRayHit, edgeTwoMiddle));
            });

            return directions.First();
        }

        private HexDirection GetSextantOfMouse(IHexCell cell) {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if(Physics.Raycast(mouseRay, out hit) && Grid.HasCellAtLocation(hit.point)) {
                return GetNearestEdgeOfCellFromPoint(cell, hit.point);

            }else {
                Debug.LogWarning("Raycast did not hit terrain");
                return HexDirection.NE;
            }
        }

        private bool HasMouseMovedThisFrame() {
            return Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0;
        }

        private RiverFlow? GetFlowFromSextants(HexDirection current, HexDirection next) {
            if(next == current.Next()) {
                return RiverFlow.Clockwise;
            }else if(next == current.Previous()) {
                return RiverFlow.Counterclockwise;
            }else {
                return null;
            }
        }

        private RiverFlow? GetFlowFromEdgeMidpoint(IHexCell cell, HexDirection sextant) {
            Vector2 toSextantMidpointFromCenter = HexMetrics.GetOuterEdgeMiddle(sextant);

            Vector2 toMouseFromCenter = GetMouseRaycastPoint() - cell.Position;

            var midpointCCWAngle = toSextantMidpointFromCenter.GetCounterclockwiseAngleBetween(toMouseFromCenter);

            if(midpointCCWAngle < 180) {
                return RiverFlow.Counterclockwise;
            }else {
                return RiverFlow.Clockwise;
            }
        }

        private Vector3 GetMouseRaycastPoint() {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if(Physics.Raycast(mouseRay, out hit) && Grid.HasCellAtLocation(hit.point)) {
                return hit.point;
            }else {
                return Vector3.zero;
            }
        }

        #endregion
        
    }

}
