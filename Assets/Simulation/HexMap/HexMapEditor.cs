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

        public bool IsPaintingTerrain { get; set; }
        public bool IsPaintingShape   { get; set; }
        public bool IsPaintingFeature { get; set; }

        private TerrainType ActiveTerrain;

        private TerrainShape ActiveShape;

        private TerrainFeature ActiveFeature;

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
            ActiveTerrain = (TerrainType)index;
        }

        public void SelectShape(int index) {
            ActiveShape = (TerrainShape)index;
        }

        public void SelectFeature(int index) {
            ActiveFeature = (TerrainFeature)index;
        }

        private void HandleInput() {
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(inputRay, out hit) && HexGrid.HasCellAtLocation(hit.point)) {
                var cellToDraw = HexGrid.GetCellAtLocation(hit.point);
                EditCell(cellToDraw);                
            }
        }

        private void EditCell(IHexCell cell) {
            if(IsPaintingTerrain) {
                cell.Terrain = ActiveTerrain;
                cell.Color = TileConfig.ColorsOfTerrains[(int)cell.Terrain];
            }

            if(IsPaintingShape) {
                cell.Shape = ActiveShape;
                cell.Elevation = TileConfig.ElevationsOfShapes[(int)cell.Shape];
            }

            if(IsPaintingFeature) {
                cell.Feature = ActiveFeature;
            }

            HexGrid.Refresh();
        }

        #endregion

    }

}
