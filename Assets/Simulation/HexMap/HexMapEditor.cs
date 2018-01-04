using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Simulation.HexMap {

    public class HexMapEditor : MonoBehaviour {

        #region instance fields and properties

        public bool IsPaintingTerrain { get; set; }
        public bool IsPaintingShape   { get; set; }
        public bool IsPaintingFeature { get; set; }

        [SerializeField] private HexGrid HexGrid;

        private TerrainType ActiveTerrain;

        private TerrainShape ActiveShape;

        private TerrainFeature ActiveFeature;

        #endregion

        #region instance methods

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
            if(Physics.Raycast(inputRay, out hit)) {
                if(IsPaintingTerrain) {
                    HexGrid.PaintCellTerrain(hit.point, ActiveTerrain);
                }
                if(IsPaintingShape) {
                    HexGrid.PaintCellShape(hit.point, ActiveShape);
                }
                if(IsPaintingFeature) {
                    HexGrid.PaintCellFeature(hit.point, ActiveFeature);
                }
            }
        }  

        #endregion

    }

}
