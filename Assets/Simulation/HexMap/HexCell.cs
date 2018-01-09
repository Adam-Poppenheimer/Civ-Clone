using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class HexCell : MonoBehaviour, IHexCell {

        #region instance fields and properties

        #region from IHexCell

        public HexCoordinates Coordinates {
            get { return _coordinates; }
            set { _coordinates = value; }
        }
        [SerializeField] private HexCoordinates _coordinates;

        public TerrainType Terrain {
            get { return _terrain; }
            set { _terrain = value; }
        }
        [SerializeField] private TerrainType _terrain;

        public TerrainShape Shape {
            get { return _shape; }
            set { _shape = value; }
        }
        [SerializeField] private TerrainShape _shape;

        public TerrainFeature Feature {
            get { return _feature; }
            set { _feature = value; }
        }
        [SerializeField] private TerrainFeature _feature;

        public int Elevation {
            get { return _elevation; }
            set {
                if(_elevation == value) {
                    return;
                }
                _elevation = value;
                var localPosition = transform.localPosition;

                localPosition.y = _elevation * HexMetrics.ElevationStep;
                localPosition.y += (NoiseGenerator.SampleNoise(localPosition).y * 2f - 1f) *
                    HexMetrics.ElevationPerturbStrength;

                transform.localPosition = localPosition;

                ValidateRivers();

                foreach(var direction in EnumUtil.GetValues<HexDirection>()){
                    if(Roads[(int)direction] && GetElevationDifference(direction) > 1){
                        SetRoad(direction, false);
                    }
                }

                Refresh();
            }
        }
        [SerializeField] private int _elevation = int.MinValue;

        public bool HasRiver {
            get { return HasIncomingRiver || HasOutgoingRiver; }
        }

        public bool HasRiverBeginOrEnd {
            get { return HasIncomingRiver != HasOutgoingRiver; }
        }

        public bool HasIncomingRiver { get; set; }

        public bool HasOutgoingRiver { get; set; }

        public HexDirection IncomingRiver { get; set; }

        public HexDirection OutgoingRiver { get; set; }

        public HexDirection RiverBeginOrEndDirection {
            get {
                return HasIncomingRiver ? IncomingRiver : OutgoingRiver;
            }
        }

        public float StreamBedY {
            get {
                return (Elevation + HexMetrics.StreamBedElevationOffset) * HexMetrics.ElevationStep;
            }
        }

        public float RiverSurfaceY {
            get {
                return (Elevation + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;
            }
        }

        public bool HasRoads {
            get { return Roads.Contains(true); }
        }

        public int WaterLevel {
            get { return _waterLevel; }
            set {
                if(_waterLevel == value) {
                    return;
                }

                _waterLevel = value;
                ValidateRivers();
                Refresh();
            }
        }
        private int _waterLevel;

        public bool IsUnderwater {
            get { return WaterLevel > Elevation; }
        }

        public float WaterSurfaceY {
            get {
                return (WaterLevel + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;
            }
        }

        public IWorkerSlot WorkerSlot { get; set; }

        public bool SuppressSlot { get; set; }

        public Color Color {
            get { return _color; }
            set {
                if(_color == value) {
                    return;
                }

                _color = value;
                Refresh();
            }
        }
        private Color _color;

        public HexGridChunk Chunk { get; set; }

        #endregion

        [SerializeField] private bool[] Roads;

        private Canvas Canvas;

        private ITileResourceLogic ResourceLogic;
        private INoiseGenerator NoiseGenerator;
        private IHexGrid Grid;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITileResourceLogic resourceLogic,
            INoiseGenerator noiseGenerator, IHexGrid grid
        ){
            WorkerSlot = new WorkerSlot(resourceLogic.GetYieldOfTile(this));
            NoiseGenerator = noiseGenerator;
            Grid = grid;
        }

        #region Unity messages

        private void Awake() {
            Canvas = GetComponentInChildren<Canvas>();
        }

        #endregion

        #region from Object

        public override string ToString() {
            return string.Format("HexCell {0}", Coordinates);
        }

        #endregion

        #region from IHexCell

        public HexEdgeType GetEdgeType(IHexCell otherCell) {
            return HexMetrics.GetEdgeType(Elevation, otherCell.Elevation);
        }

        public void ToggleUI(bool isVisible) {
            if(Canvas != null) {
                Canvas.gameObject.SetActive(isVisible);
            }
        }

        public bool HasRiverThroughEdge(HexDirection direction) {
            return (HasIncomingRiver && IncomingRiver == direction)
                || (HasOutgoingRiver && OutgoingRiver == direction);
        }

        public void SetOutgoingRiver(HexDirection direction) {
            if(HasOutgoingRiver && OutgoingRiver == direction) {
                return;
            }

            IHexCell neighbor = Grid.GetNeighbor(this, direction);
            if(!IsValidRiverDestination(neighbor)) {
                return;
            }

            RemoveOutgoingRiver();
            if(HasIncomingRiver && IncomingRiver == direction) {
                RemoveIncomingRiver();
            }

            HasOutgoingRiver = true;
            OutgoingRiver = direction;

            neighbor.RemoveIncomingRiver();
            neighbor.HasIncomingRiver = true;
            neighbor.IncomingRiver = direction.Opposite();

            SetRoad(direction, false);
        }

        public void RemoveOutgoingRiver() {
            if(!HasOutgoingRiver) {
                return;
            }

            HasOutgoingRiver = false;
            RefreshSelfOnly();

            IHexCell neighbor = Grid.GetNeighbor(this, OutgoingRiver);
            neighbor.HasOutgoingRiver = false;
            neighbor.RefreshSelfOnly();
        }

        public void RemoveIncomingRiver() {
            if(!HasIncomingRiver) {
                return;
            }
            HasIncomingRiver = false;
            RefreshSelfOnly();

            IHexCell neighbor = Grid.GetNeighbor(this, IncomingRiver);
            neighbor.HasOutgoingRiver = false;
            neighbor.RefreshSelfOnly();
        }

        public void RemoveRiver() {
            RemoveOutgoingRiver();
            RemoveIncomingRiver();
        }

        public bool IsValidRiverDestination(IHexCell neighbor) {
            return neighbor != null
                && (Elevation >= neighbor.Elevation || WaterLevel == neighbor.Elevation);
        }

        public bool HasRoadThroughEdge(HexDirection direction) {
            return Roads[(int)direction];
        }

        public void AddRoad(HexDirection direction) {
            if(!Roads[(int)direction] && !HasRiverThroughEdge(direction) &&
                GetElevationDifference(direction) <= 1
            ){
                SetRoad(direction, true);
            }
        }

        public void RemoveRoads() {
            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                if(Roads[(int)direction]) {
                    SetRoad(direction, false);
                }                
            }
        }

        public int GetElevationDifference(HexDirection direction) {
            if(Grid.HasNeighbor(this, direction)) {
                var neighbor = Grid.GetNeighbor(this, direction);
                return Math.Abs(Elevation - neighbor.Elevation);
            }else {
                return 0;
            }
        }

        private void ValidateRivers() {
            if( HasOutgoingRiver &&
                !IsValidRiverDestination(Grid.GetNeighbor(this, OutgoingRiver))
            ) {
                RemoveOutgoingRiver();
            }

            if( HasIncomingRiver &&
                !Grid.GetNeighbor(this, IncomingRiver).IsValidRiverDestination(this)
            ) {
                RemoveIncomingRiver();
            }
        }

        private void SetRoad(HexDirection direction, bool state) {
            Roads[(int)direction] = state;

            if(Grid.HasNeighbor(this, direction)) {
                var neighbor = Grid.GetNeighbor(this, direction) as HexCell;
                neighbor.Roads[(int)direction.Opposite()] = state;
                neighbor.RefreshSelfOnly();
            }

            RefreshSelfOnly();
        }

        public void Refresh() {
            if(Chunk != null) {
                Chunk.Refresh();
                foreach(var neighbor in Grid.GetNeighbors(this)) {
                    if(neighbor.Chunk != Chunk) {
                        neighbor.Chunk.Refresh();
                    }
                }
            }
        }

        public void RefreshSelfOnly() {
            Chunk.Refresh();
        }

        #endregion

        #endregion

    }

}
