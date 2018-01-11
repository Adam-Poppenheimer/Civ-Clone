using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

namespace Assets.Simulation.HexMap {

    public class HexMapEditor : MonoBehaviour {

        #region internal types

        private enum OptionalToggle {
            Ignore, Yes, No
        }

        #endregion

        #region instance fields and properties

        private bool ApplyTerrain;
        private bool ApplyFeatures;
        private bool ApplyElevation;
        private bool ApplyWaterLevel;

        private TerrainType    ActiveTerrain;
        private TerrainFeature ActiveFeature;
        private int            ActiveElevation;
        private int            ActiveWaterLevel;

        private int BrushSize;

        private OptionalToggle RiverMode, RoadMode;

        private bool IsDragging;
        HexDirection DragDirection;
        IHexCell PreviousCell;

        private IDisposable CellEnterSubscription;

        private IHexGrid HexGrid;
        private IHexGridConfig TileConfig;
        private HexCellSignals CellSignals;

        private IRiverCanon RiverCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGrid hexGrid, IHexGridConfig tileConfig,
            HexCellSignals cellSignals, IRiverCanon riverCanon
        ){
            HexGrid = hexGrid;
            TileConfig = tileConfig;
            CellSignals = cellSignals;
            RiverCanon = riverCanon;
        }

        #region Unity message methods

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
        }

        private void OnDisable() {
            IsDragging = false;
            PreviousCell = null;
            CellEnterSubscription.Dispose();
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

        public void SetApplyElevation(bool toggle) {
            ApplyElevation = toggle;
        }

        public void SetActiveElevation(float level) {
            ActiveElevation = (int)level;
        }

        public void SetApplyWaterLevel(bool toggle) {
            ApplyWaterLevel = toggle;
        }

        public void SetWaterLevel(float level) {
            ActiveWaterLevel = (int)level;
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
                cell.Color = TileConfig.ColorsOfTerrains[(int)cell.Terrain];
            }

            if(ApplyFeatures && !cell.IsUnderwater) {
                cell.Feature = ActiveFeature;
            }

            if(ApplyElevation) {
                cell.Elevation = ActiveElevation;
            }

            if(ApplyWaterLevel) {
                cell.WaterLevel = ActiveWaterLevel;
                if(cell.IsUnderwater) {
                    cell.Feature = TerrainFeature.None;
                }
            }

            if(RiverMode == OptionalToggle.No) {
                RiverCanon.RemoveRiver(cell);
            }
            if(RoadMode == OptionalToggle.No) {
                cell.RemoveRoads();
            }
            if(IsDragging && HexGrid.HasNeighbor(cell, DragDirection.Opposite())) {
                IHexCell otherCell = HexGrid.GetNeighbor(cell, DragDirection.Opposite());
                if(otherCell != null) {
                    if(RiverMode == OptionalToggle.Yes) {
                        RiverCanon.SetOutgoingRiver(otherCell, DragDirection);
                    }
                    if(RoadMode == OptionalToggle.Yes) {
                        otherCell.AddRoad(DragDirection);
                    }
                }                
            }
        }

        #endregion

    }

}
