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
                _elevation = value;
                var localPosition = transform.localPosition;
                localPosition.y = _elevation * HexMetrics.ElevationStep;
                transform.localPosition = localPosition;
            }
        }
        [SerializeField] private int _elevation;

        public IWorkerSlot WorkerSlot { get; private set; }

        public bool SuppressSlot { get; set; }

        public Color Color { get; set; }        

        #endregion

        private ITileResourceLogic ResourceLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITileResourceLogic resourceLogic) {
            WorkerSlot = new WorkerSlot(resourceLogic.GetYieldOfTile(this));
        }

        #region from IHexCell

        public HexEdgeType GetEdgeType(IHexCell otherCell) {
            return HexMetrics.GetEdgeType(Elevation, otherCell.Elevation);
        }

        #endregion

        #endregion

    }

}
