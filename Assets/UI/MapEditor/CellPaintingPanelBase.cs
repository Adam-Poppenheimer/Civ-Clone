using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public abstract class CellPaintingPanelBase : MonoBehaviour {

        #region instance fields and properties
        
        public int BrushSize { get; set; }

        private bool         IsDragging;
        private HexDirection DragDirection;
        private IHexCell     PreviousCell;

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private IHexGrid       Grid;
        private HexCellSignals CellSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IHexGrid grid, HexCellSignals cellSignals) {
            Grid        = grid;
            CellSignals = cellSignals;
        }

        #region Unity messages

        private void OnEnable() {
            SignalSubscriptions.Add(CellSignals.BeginDragSignal   .Subscribe(OnCellBeginDrag));
            SignalSubscriptions.Add(CellSignals.EndDragSignal     .Subscribe(OnCellEndDrag));
            SignalSubscriptions.Add(CellSignals.PointerDownSignal .Subscribe(OnCellPointerDown));
            SignalSubscriptions.Add(CellSignals.PointerEnterSignal.Subscribe(OnCellPointerEnter));
        }

        private void OnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());

            SignalSubscriptions.Clear();
        }

        #endregion

        private void OnCellBeginDrag(HexCellDragData dragData) {
            IsDragging = true;
            EditCells(dragData.CellBeingDragged);
        }

        private void OnCellEndDrag(HexCellDragData dragData) {
            IsDragging = false;
        }

        private void OnCellPointerDown(Tuple<IHexCell, PointerEventData> data) {
            EditCells(data.Item1);
        }

        private void OnCellPointerEnter(IHexCell cell) {
            if(IsDragging) {
                EditCells(cell);
            }
        }

        private void EditCells(IHexCell center) {
            foreach(var cell in Grid.GetCellsInRadius(center, BrushSize)) {
                EditCell(cell);
            }
        }

        protected abstract void EditCell(IHexCell cell);

        #endregion

    }

}
