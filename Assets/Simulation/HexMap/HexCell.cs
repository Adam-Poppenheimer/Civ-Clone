using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.HexMap;
using Assets.Simulation.WorkerSlots;
using Assets.Simulation.MapRendering;

namespace Assets.Simulation.HexMap {

    public class HexCell : IHexCell {

        #region instance fields and properties

        #region from IHexCell

        public Vector3 AbsolutePosition {
            get { return Grid.GetAbsolutePositionFromRelative(GridRelativePosition); }
        }

        public Vector2 AbsolutePositionXZ {
            get {
                var position = AbsolutePosition;
                return new Vector2(position.x, position.z);
            }
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

        public IWorkerSlot WorkerSlot { get; set; }

        public bool SuppressSlot { get; set; }

        public int Index { get; set; }

        public bool IsRoughTerrain {
            get { return Vegetation == CellVegetation.Forest || Vegetation == CellVegetation.Jungle || Shape == CellShape.Hills; }
        }

        public Vector3 OverlayAnchorPoint {
            get {
                return Grid.PerformIntersectionWithTerrainSurface(GridRelativePosition) + Vector3.up * 0.5f;
            }
        }

        #endregion

        private IMapChunk[] OverlappingChunks;




        private IHexGrid       Grid;
        private HexCellSignals Signals;

        #endregion

        #region constructors

        public HexCell(
            Vector3 gridRelativePosition, IHexGrid grid, HexCellSignals signals
        ) {
            GridRelativePosition = gridRelativePosition;

            Grid    = grid;
            Signals = signals;
        }

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format("HexCell {0}", Coordinates);
        }

        #endregion

        #region from IHexCell

        public void AttachToChunks(IMapChunk[] chunks) {
            OverlappingChunks = chunks;
        }

        public void SetMapData(float data) {
            
        }

        public void Refresh() {
            RefreshSelfOnly();

            foreach(var neighbor in Grid.GetNeighbors(this)) {
                neighbor.RefreshSelfOnly();
            }
        }

        public void RefreshSelfOnly() {
            if(OverlappingChunks != null) {
                foreach(var chunk in OverlappingChunks) {
                    chunk.RefreshAll();
                }
            }
        }

        public void RefreshVisibility() {
            
        }

        #endregion

        #endregion

    }

}
