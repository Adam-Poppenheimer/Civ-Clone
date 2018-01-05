using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

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

                Refresh();
            }
        }
        [SerializeField] private int _elevation = int.MinValue;

        public IWorkerSlot WorkerSlot { get; private set; }

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

        #region from IHexCell

        public HexEdgeType GetEdgeType(IHexCell otherCell) {
            return HexMetrics.GetEdgeType(Elevation, otherCell.Elevation);
        }

        public void ToggleUI(bool isVisible) {
            if(Canvas != null) {
                Canvas.gameObject.SetActive(isVisible);
            }
        }

        #endregion

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

        #endregion

    }

}
