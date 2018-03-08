using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.HexMap;

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
            set {
                _terrain = value;
                RefreshSlot();
                ShaderData.RefreshTerrain(this);
            }
        }
        [SerializeField] private TerrainType _terrain;

        public TerrainFeature Feature {
            get { return _feature; }
            set {
                if(value == _feature) {
                    return;
                }

                _feature = value;
                RefreshSlot();
                RefreshSelfOnly();
            }
        }
        [SerializeField] private TerrainFeature _feature;

        public TerrainShape Shape {
            get { return _shape; }
            set {
                _shape = value;
                Refresh();
                RefreshSlot();
                ShaderData.RefreshTerrain(this);
            }
        }
        [SerializeField] private TerrainShape _shape;

        public int FoundationElevation {
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

                RiverCanon.ValidateRivers(this);

                Refresh();
            }
        }
        [SerializeField] private int _elevation = int.MinValue;

        public int EdgeElevation {
            get {
                switch(Shape) {
                    case TerrainShape.Flatlands: return FoundationElevation;
                    case TerrainShape.Hills:     return FoundationElevation + Mathf.RoundToInt(HexMetrics.HillEdgeElevation);
                    case TerrainShape.Mountains: return FoundationElevation + Mathf.RoundToInt(HexMetrics.MountainEdgeElevation);
                    default:                     return FoundationElevation;
                }
            }
        }

        public int PeakElevation {
            get {
                switch(Shape) {
                    case TerrainShape.Flatlands: return FoundationElevation;
                    case TerrainShape.Hills:     return FoundationElevation + Mathf.RoundToInt(HexMetrics.HillPeakElevation);
                    case TerrainShape.Mountains: return FoundationElevation + Mathf.RoundToInt(HexMetrics.MountainPeakElevation);
                    default:                     return FoundationElevation;
                }
            }
        }

        public float StreamBedY {
            get {
                return (FoundationElevation + HexMetrics.StreamBedElevationOffset) * HexMetrics.ElevationStep;
            }
        }

        public float RiverSurfaceY {
            get {
                return (FoundationElevation + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;
            }
        }

        public bool HasRoads {
            get { return _hasRoads; }
            set {
                if(_hasRoads != value) {
                    _hasRoads = value;
                    Refresh();
                }
            }
        }
        private bool _hasRoads;

        public int WaterLevel {
            get { return _waterLevel; }
            set {
                if(_waterLevel == value) {
                    return;
                }

                _waterLevel = value;
                RiverCanon.ValidateRivers(this);
                Refresh();
            }
        }
        private int _waterLevel;

        public bool IsUnderwater {
            get { return WaterLevel > FoundationElevation; }
        }

        public float WaterSurfaceY {
            get {
                return (WaterLevel + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;
            }
        }

        public IWorkerSlot WorkerSlot {
            get { return workerSlot; }
        }
        private WorkerSlot workerSlot = new WorkerSlot(ResourceSummary.Empty);

        public bool SuppressSlot { get; set; }

        public HexGridChunk Chunk { get; set; }

        public HexCellShaderData ShaderData { get; set; }

        public int Index { get; set; }

        public IHexCellOverlay Overlay {
            get { return overlay; }
        }
        [SerializeField] private HexCellOverlay overlay;

        #endregion

        private ICellResourceLogic ResourceLogic;
        private INoiseGenerator NoiseGenerator;
        private IHexGrid Grid;
        private IRiverCanon RiverCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICellResourceLogic resourceLogic,
            INoiseGenerator noiseGenerator, IHexGrid grid, IRiverCanon riverCanon
        ){
            ResourceLogic = resourceLogic;
            NoiseGenerator = noiseGenerator;
            Grid = grid;
            RiverCanon = riverCanon;
        }

        #region Unity messages

        private void Awake() {
            overlay.Parent = this;
        }

        #endregion

        #region from Object

        public override string ToString() {
            return string.Format("HexCell {0}", Coordinates);
        }

        #endregion

        #region from IHexCell

        public HexEdgeType GetEdgeType(IHexCell otherCell) {
            return HexMetrics.GetEdgeType(this, otherCell);
        }

        public int GetElevationDifference(HexDirection direction) {
            if(Grid.HasNeighbor(this, direction)) {
                var neighbor = Grid.GetNeighbor(this, direction);
                return Math.Abs(FoundationElevation - neighbor.FoundationElevation);
            }else {
                return 0;
            }
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

        public void RefreshVisibility() {
            ShaderData.RefreshVisibility(this);
        }

        public void RefreshSlot() {
            workerSlot.BaseYield = ResourceLogic.GetYieldOfCell(this);
        }

        #endregion

        #endregion

    }

}
