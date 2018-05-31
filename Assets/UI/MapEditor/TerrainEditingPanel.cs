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

        private List<IDisposable> CellSignalSubscriptions = new List<IDisposable>();




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
            CellSignalSubscriptions.Add(CellSignals.PointerEnterSignal.AsObservable.Subscribe(delegate(IHexCell cell) {
                if(Input.GetMouseButton(0)) {
                    HandleInput();
                }
            }));

            CellSignalSubscriptions.Add(CellSignals.PointerDownSignal         .Subscribe(data => HandleInput()));
            CellSignalSubscriptions.Add(CellSignals.ClickedSignal.AsObservable.Subscribe(data => HandleInput()));
            CellSignalSubscriptions.Add(CellSignals.DraggedSignal             .Subscribe(data => HandleInput()));
        }

        private void OnDisable() {
            IsDragging = false;
            PreviousCell = null;

            foreach(var subscription in CellSignalSubscriptions) {
                subscription.Dispose();
            }

            CellSignalSubscriptions.Clear();
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

                EditCells(currentCell, hit.point);   
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

        private void EditCells(IHexCell center, Vector3 mouseRayHit) {
            foreach(var cell in HexGrid.GetCellsInRadius(center, BrushSize)) {
                EditCell(cell, mouseRayHit);
            }
        }

        private void EditCell(IHexCell cell, Vector3 mouseRayHit) {
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

            //This solution has problems when the pointer crosses a chunk boundary,
            //which is considered acceptable for the time-being.
            if(RiverMode == OptionalToggle.No) {
                var nearestDirection = GetNearestEdgeOfCellFromPoint(cell, mouseRayHit);

                RiverCanon.RemoveRiverFromCellInDirection(cell, nearestDirection);
            }else if(RiverMode == OptionalToggle.Yes) {
                var nearestDirection = GetNearestEdgeOfCellFromPoint(cell, mouseRayHit);

                if(RiverCanon.CanAddRiverToCell(cell, nearestDirection, RiverFlow.Clockwise)) {
                    RiverCanon.AddRiverToCell(cell, nearestDirection, RiverFlow.Clockwise);
                }
            }

            RiverCanon.ValidateRivers(cell);
        }

        private HexDirection GetNearestEdgeOfCellFromPoint(IHexCell cell, Vector3 mouseRayHit) {
            var directions = EnumUtil.GetValues<HexDirection>().ToList();

            directions.Sort(delegate(HexDirection directionOne, HexDirection directionTwo) {
                var edgeOneMiddle = cell.transform.position + HexMetrics.GetOuterEdgeMiddle(directionOne);
                var edgeTwoMiddle = cell.transform.position + HexMetrics.GetOuterEdgeMiddle(directionTwo);

                return Vector3.Distance(mouseRayHit, edgeOneMiddle)
                    .CompareTo(Vector3.Distance(mouseRayHit, edgeTwoMiddle));
            });

            return directions.First();
        }

        #endregion

    }

}
