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

                var signalData = new HexPropertyChangedData<CellTerrain>(this, _terrain, value);

                _terrain = value;

                Signals.TerrainChanged.OnNext(signalData);
            }
        }
        private CellTerrain _terrain;

        public CellShape Shape {
            get { return _shape; }
            set {
                if(_shape == value) {
                    return;
                }

                var signalData = new HexPropertyChangedData<CellShape>(this, _shape, value);

                _shape = value;

                Signals.ShapeChanged.OnNext(signalData);
            }
        }
        private CellShape _shape;

        public CellVegetation Vegetation {
            get { return _vegetation; }
            set {
                if(value == _vegetation) {
                    return;
                }

                var signalData = new HexPropertyChangedData<CellVegetation>(this, _vegetation, value);

                _vegetation = value;

                Signals.VegetationChanged.OnNext(signalData);
            }
        }
        private CellVegetation _vegetation;

        public CellFeature Feature {
            get { return _feature; }
            set {
                if(_feature == value) {
                    return;
                }

                var signalData = new HexPropertyChangedData<CellFeature>(this, _feature, value);

                _feature = value;

                Signals.FeatureChanged.OnNext(signalData);
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

                    var signalData = new HexPropertyChangedData<bool>(this, _hasRoads, value);

                    _hasRoads = value;

                    Signals.RoadStatusChanged.OnNext(signalData);
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

        public IEnumerable<IMapChunk> OverlappingChunks {
            get { return overlappingChunks; }
        }
        private IMapChunk[] overlappingChunks;

        #endregion




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
            overlappingChunks = chunks;
        }

        public void SetMapData(float data) {
            
        }

        #endregion

        #endregion

    }

}
