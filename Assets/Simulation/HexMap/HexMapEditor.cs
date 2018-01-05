using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexMapEditor : MonoBehaviour {

        #region instance fields and properties

        private bool ApplyTerrain;
        private bool ApplyShape;
        private bool ApplyFeature;

        private TerrainType ActiveTerrain;

        private TerrainShape ActiveShape;

        private TerrainFeature ActiveFeature;

        private int BrushSize;

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
            if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
                HandleInput();
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

        private void HandleInput() {
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(inputRay, out hit) && HexGrid.HasCellAtLocation(hit.point)) {
                var cellToDraw = HexGrid.GetCellAtLocation(hit.point);
                EditCells(cellToDraw);                               
            }
        }

        private void EditCells(IHexCell center) {
            foreach(var cell in HexGrid.GetCellsInRadius(center, BrushSize)) {
                EditCell(cell);
            }
        }

        private void EditCell(IHexCell cell) {
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
        }

        #endregion

    }

}
