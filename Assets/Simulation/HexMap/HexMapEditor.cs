using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexMapEditor : MonoBehaviour {

        #region internal types

        private enum OptionalToggle {
            Ignore, Yes, No
        }

        #endregion

        #region instance fields and properties

        private bool ApplyTerrain;
        private bool ApplyShape;
        private bool ApplyFeature;

        private TerrainType ActiveTerrain;

        private TerrainShape ActiveShape;

        private TerrainFeature ActiveFeature;

        private int BrushSize;

        private OptionalToggle RiverMode;

        private bool IsDragging;
        HexDirection DragDirection;
        IHexCell PreviousCell;

        private IHexGrid HexGrid;
        private ITileConfig TileConfig;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IHexGrid hexGrid, ITileConfig tileConfig) {
            HexGrid = hexGrid;
            TileConfig = tileConfig;
        }

        #region Unity message methods

        private void Awake() {
            SelectTerrain(0);
        }

        private void Update() {
            if(Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
                HandleInput();
            }else {
                PreviousCell = null;
            }
        }

        #endregion

        public void SelectTerrain(int index) {
            ApplyTerrain = index >= 0;
            if(ApplyTerrain) {
                ActiveTerrain = (TerrainType)index;
            }            
        }

        public void SelectShape(int index) {
            ApplyShape = index >= 0;
            if(ApplyShape) {
                ActiveShape = (TerrainShape)index;
            }
        }

        public void SelectFeature(int index) {
            ApplyFeature = index >= 0;
            if(ApplyFeature) {
                ActiveFeature = (TerrainFeature)index;
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
                if(HexGrid.GetNeighbor(PreviousCell, DragDirection) == currentCell) {
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

            if(ApplyShape) {
                cell.Shape = ActiveShape;
                cell.Elevation = TileConfig.ElevationsOfShapes[(int)cell.Shape];
            }

            if(ApplyFeature) {
                cell.Feature = ActiveFeature;
            }

            if(RiverMode == OptionalToggle.No) {
                cell.RemoveRiver();
            }else if(IsDragging && RiverMode == OptionalToggle.Yes) {
                IHexCell otherCell = HexGrid.GetNeighbor(cell, DragDirection.Opposite());
                if(otherCell != null) {
                    otherCell.SetOutgoingRiver(DragDirection);
                }                
            }
        }

        #endregion

    }

}
