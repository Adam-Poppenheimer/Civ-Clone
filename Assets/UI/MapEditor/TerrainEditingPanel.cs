using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.UI.MapEditor {

    public class TerrainEditingPanel : MonoBehaviour {

        #region internal types

        private enum OptionalToggle {
            Ignore, Yes, No
        }

        #endregion

        #region instance fields and properties

        private bool ApplyTerrain;
        private bool ApplyFeatures;
        private bool ApplyShape;

        private TerrainType    ActiveTerrain;
        private TerrainFeature ActiveFeature;
        private TerrainShape   ActiveShape;

        private int BrushSize;

        private OptionalToggle RiverMode, RoadMode;

        private bool IsDragging;
        HexDirection DragDirection;
        IHexCell PreviousCell;

        private IDisposable CellEnterSubscription;
        private IDisposable CellPointerDownSubscription;
        private IDisposable CellClickedSubscription;




        private IHexGrid       HexGrid;
        private HexCellSignals CellSignals;
        private IRiverCanon    RiverCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGrid hexGrid, HexCellSignals cellSignals, IRiverCanon riverCanon
        ){
            HexGrid     = hexGrid;
            CellSignals = cellSignals;
            RiverCanon  = riverCanon;
        }

        #region Unity messages

        private void Awake() {
            SelectTerrain(0);
        }

        private void Update() {
            if(!Input.GetMouseButton(0)) {
                PreviousCell = null;
            }
        }

        private void OnEnable() {
            CellEnterSubscription = CellSignals.PointerEnterSignal.AsObservable.Subscribe(delegate(IHexCell cell) {
                if(Input.GetMouseButton(0)) {
                    HandleInput();
                }
            });

            CellPointerDownSubscription = CellSignals.PointerDownSignal         .Subscribe(data => HandleInput());
            CellClickedSubscription     = CellSignals.ClickedSignal.AsObservable.Subscribe(data => HandleInput());
        }

        private void OnDisable() {
            IsDragging = false;
            PreviousCell = null;

            CellEnterSubscription  .Dispose();
            CellPointerDownSubscription   .Dispose();
            CellClickedSubscription.Dispose();
        }

        #endregion

        public void SelectTerrain(int index) {
            ApplyTerrain = index >= 0;
            if(ApplyTerrain) {
                ActiveTerrain = (TerrainType)index;
            }            
        }

        public void SelectFeature(int index) {
            ApplyFeatures = index >= 0;
            if(ApplyFeatures) {
                ActiveFeature = (TerrainFeature)index;
            }
        }

        public void SelectShape(int index) {
            ApplyShape = index >= 0;
            if(ApplyShape) {
                ActiveShape = (TerrainShape)index;
            }
        }

        public void SelectBrushSize(float size) {
            BrushSize = (int)size;
        }

        public void ToggleUI(bool isVisible) {
            HexGrid.ToggleUI(isVisible);
        }

        public void SetRiverMode(int mode) {
            RiverMode = (OptionalToggle)mode;
        }

        public void SetRoadMode(int mode) {
            RoadMode = (OptionalToggle)mode;
        }

        private void HandleInput() {
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(inputRay, out hit) && HexGrid.HasCellAtLocation(hit.point)) {
                IHexCell currentCell = HexGrid.GetCellAtLocation(hit.point);

                if(PreviousCell != null && PreviousCell != currentCell){
                    ValidateDrag(currentCell);
                }else{
                    IsDragging = false;
                }

                EditCells(currentCell);   
                PreviousCell = currentCell;                            
            }else {
                PreviousCell = null;
            }
        }

        private void ValidateDrag(IHexCell currentCell) {
            for(DragDirection = HexDirection.NE; DragDirection <= HexDirection.NW; DragDirection++) {
                if( HexGrid.HasNeighbor(PreviousCell, DragDirection) &&
                    HexGrid.GetNeighbor(PreviousCell, DragDirection) == currentCell
                ){
                    IsDragging = true;
                    return;
                }
            }
            IsDragging = false;
        }

        private void EditCells(IHexCell center) {
            foreach(var cell in HexGrid.GetCellsInRadius(center, BrushSize)) {
                EditCell(cell);
            }
        }

        private void EditCell(IHexCell cell) {
            if(cell == null) {
                return;
            }

            if(ApplyTerrain) {
                cell.Terrain = ActiveTerrain;
            }

            if(ApplyFeatures && !cell.IsUnderwater) {
                cell.Feature = ActiveFeature;
            }

            if(ApplyShape) {
                cell.Shape = ActiveShape;
            }

            if(cell.IsUnderwater) {
                cell.Feature = TerrainFeature.None;
            }

            if(RoadMode == OptionalToggle.No) {
                cell.HasRoads = false;
            }else if(RoadMode == OptionalToggle.Yes) {
                cell.HasRoads = true;
            }

            if(RiverMode == OptionalToggle.No) {
                RiverCanon.RemoveAllRiversFromCell(cell);

            }else if(RiverMode == OptionalToggle.Yes) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    if(RiverCanon.CanAddRiverToCell(cell, direction, RiverDirection.Clockwise)) {
                        RiverCanon.AddRiverToCell(cell, direction, RiverDirection.Clockwise);
                    }
                }
            }
        }

        #endregion

    }

}
