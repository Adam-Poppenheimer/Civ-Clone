﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.HexMap;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.HexMap {

    public class HexCell : MonoBehaviour, IHexCell {

        #region instance fields and properties

        #region from IHexCell

        public Vector3 Position {
            get { return transform.position; }
        }

        public Vector3 LocalPosition {
            get { return transform.localPosition; }
        }

        public HexCoordinates Coordinates { get; set; }

        public CellTerrain Terrain {
            get { return _terrain; }
            set {
                _terrain = value;

                ShaderData.RefreshTerrain(this);
            }
        }
        private CellTerrain _terrain;

        public CellVegetation Vegetation {
            get { return _vegetation; }
            set {
                if(value == _vegetation) {
                    return;
                }

                _vegetation = value;
                RefreshSelfOnly();
                Signals.VegetationChangedSignal.OnNext(this);
            }
        }
        private CellVegetation _vegetation;

        public CellShape Shape {
            get { return _shape; }
            set {
                if(_shape == value) {
                    return;
                }

                _shape = value;
                Refresh();
                ShaderData.RefreshTerrain(this);
                Signals.ShapeChangedSignal.OnNext(this);
            }
        }
        private CellShape _shape;

        public int FoundationElevation {
            get { return _elevation; }
            set {
                if(_elevation == value) {
                    return;
                }
                _elevation = value;
                var localPosition = transform.localPosition;

                localPosition.y = _elevation * HexMetrics.ElevationStep;
                localPosition.y += (NoiseGenerator.SampleNoise(localPosition).y * 2f) - 1f;

                transform.localPosition = localPosition;

                RiverCanon.ValidateRivers(this);

                Refresh();

                Signals.FoundationElevationChangedSignal.OnNext(this);
            }
        }
        private int _elevation;

        public int EdgeElevation {
            get {
                return FoundationElevation + Config.GetEdgeElevationForShape(Shape);
            }
        }

        public int PeakElevation {
            get {
                return FoundationElevation + Config.GetPeakElevationForShape(Shape);
            }
        }

        public float PeakY {
            get { return transform.localPosition.y + (Config.GetPeakElevationForShape(Shape) * HexMetrics.ElevationStep); }
        }

        public float EdgeY {
            get { return transform.localPosition.y + (Config.GetEdgeElevationForShape(Shape) * HexMetrics.ElevationStep); }
        }

        public float StreamBedY {
            get {
                return (FoundationElevation + HexMetrics.StreamBedElevationOffset) * HexMetrics.ElevationStep;
            }
        }

        public float RiverSurfaceY {
            get {
                if(Shape == CellShape.Flatlands) {
                    return WaterSurfaceY;
                }else {
                    return EdgeY + (HexMetrics.RiverElevationOffset * HexMetrics.ElevationStep);
                }
            }
        }

        public bool RequiresYPerturb {
            get { return Shape == CellShape.Hills; }
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

        public float WaterSurfaceY {
            get {
                return (Config.WaterLevel + HexMetrics.OceanElevationOffset) * HexMetrics.ElevationStep;
            }
        }

        public int ViewElevation {
            get { return PeakElevation >= Config.WaterLevel ? PeakElevation : Config.WaterLevel; }
        }

        public IWorkerSlot WorkerSlot { get; set; }

        public bool SuppressSlot { get; set; }

        public HexGridChunk Chunk { get; set; }

        public HexCellShaderData ShaderData { get; set; }

        public int Index { get; set; }

        public IHexCellOverlay Overlay {
            get { return overlay; }
        }
        [SerializeField] private HexCellOverlay overlay;

        public bool IsRoughTerrain {
            get { return Vegetation == CellVegetation.Forest || Vegetation == CellVegetation.Jungle || Shape == CellShape.Hills; }
        }

        public Vector3 UnitAnchorPoint {
            get {
                var retval = transform.position;
                retval.y += HexMetrics.ElevationStep * (PeakElevation - FoundationElevation);
                return retval;
            }
        }

        #endregion

        private INoiseGenerator NoiseGenerator;
        private IHexGrid        Grid;
        private IRiverCanon     RiverCanon;
        private HexCellSignals  Signals;
        private IHexMapConfig   Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            INoiseGenerator noiseGenerator, IHexGrid grid, IRiverCanon riverCanon,
            HexCellSignals signals, IHexMapConfig config
        ){
            NoiseGenerator = noiseGenerator;
            Grid           = grid;
            RiverCanon     = riverCanon;
            Signals        = signals;
            Config         = config;
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

        public void Destroy() {
            if(Application.isPlaying) {
                Destroy(gameObject);
            }else {
                DestroyImmediate(gameObject);
            }
        }

        #endregion

        #endregion

    }

}
