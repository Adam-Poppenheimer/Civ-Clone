using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.HexMap;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.HexMap {

    public class HexCell : IHexCell {

        #region instance fields and properties

        #region from IHexCell

        public Vector3 AbsolutePosition {
            get { return Grid.GetAbsolutePositionFromRelative(GridRelativePosition); }
        }

        public Vector3 GridRelativePosition { get; private set; }

        public HexCoordinates Coordinates { get; set; }

        public CellTerrain Terrain {
            get { return _terrain; }
            set {
                if(_terrain == value) {
                    return;
                }

                _terrain = value;

                Refresh();
                ShaderData.RefreshTerrain(this);
                Signals.TerrainChangedSignal.OnNext(this);
            }
        }
        private CellTerrain _terrain;

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

        public CellFeature Feature {
            get { return _feature; }
            set {
                if(_feature == value) {
                    return;
                }

                _feature = value;
                Refresh();
            }
        }
        private CellFeature _feature;

        public int FoundationElevation {
            get { return RenderConfig.GetFoundationElevationForTerrain(Terrain); }
        }

        public int EdgeElevation {
            get {
                return FoundationElevation + RenderConfig.GetEdgeElevationForShape(Shape);
            }
        }

        public int PeakElevation {
            get {
                return FoundationElevation + RenderConfig.GetPeakElevationForShape(Shape);
            }
        }

        public float PeakY {
            get { return GridRelativePosition.y + (RenderConfig.GetPeakElevationForShape(Shape) * RenderConfig.ElevationStep); }
        }

        public float EdgeY {
            get { return GridRelativePosition.y + (RenderConfig.GetEdgeElevationForShape(Shape) * RenderConfig.ElevationStep); }
        }

        public float StreamBedY {
            get {
                return (FoundationElevation + RenderConfig.StreamBedElevationOffset) * RenderConfig.ElevationStep;
            }
        }

        public float RiverSurfaceY {
            get {
                if(Shape == CellShape.Flatlands) {
                    return WaterSurfaceY;
                }else {
                    return EdgeY + (RenderConfig.RiverElevationOffset * RenderConfig.ElevationStep);
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
                return (RenderConfig.WaterLevel + RenderConfig.OceanElevationOffset) * RenderConfig.ElevationStep;
            }
        }

        public IWorkerSlot WorkerSlot { get; set; }

        public bool SuppressSlot { get; set; }

        public HexGridChunk Chunk { get; set; }

        public HexCellShaderData ShaderData { get; set; }

        public int Index { get; set; }

        public bool IsRoughTerrain {
            get { return Vegetation == CellVegetation.Forest || Vegetation == CellVegetation.Jungle || Shape == CellShape.Hills; }
        }

        public Vector3 UnitAnchorPoint {
            get {
                return new Vector3(
                    AbsolutePosition.x,
                    AbsolutePosition.y + RenderConfig.ElevationStep * (PeakElevation - FoundationElevation),
                    AbsolutePosition.z
                );
            }
        }

        public Vector3 OverlayAnchorPoint {
            get {
                return Grid.PerformIntersectionWithTerrainSurface(GridRelativePosition) + Vector3.up * 0.5f;
            }
        }

        #endregion

        private IHexGrid            Grid;
        private HexCellSignals      Signals;
        private IHexMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        public HexCell(
            Vector3 gridRelativePosition, IHexGrid grid,
            HexCellSignals signals, IHexMapRenderConfig renderConfig
        ) {
            GridRelativePosition = gridRelativePosition;

            Grid         = grid;
            Signals      = signals;
            RenderConfig = renderConfig;
        }

        #endregion

        #region instance methods

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

        public void SetMapData(float data) {
            ShaderData.SetMapData(this, data);
        }

        public void Refresh() {
            PerformRefreshActions();

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
            PerformRefreshActions();

            Chunk.Refresh();
        }

        public void RefreshVisibility() {
            if(ShaderData != null) {
                ShaderData.RefreshVisibility(this);
            }
        }

        #endregion

        private void PerformRefreshActions() {
            var localPosition = GridRelativePosition;

            localPosition.y = FoundationElevation * RenderConfig.ElevationStep;

            GridRelativePosition = localPosition;
        }

        #endregion

    }

}
