using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.HexMap;
using UnityEngine;

namespace Assets.Simulation.Cities.Buildings {

    [Serializable]
    public struct CellYieldModificationData : ICellYieldModificationData {

        #region instance fields and properties

        #region ICellYieldModificationData

        public CellPropertyType PropertyConsidered {
            get { return _propertyConsidered; }
        }
        [SerializeField] private CellPropertyType _propertyConsidered;

        public TerrainType TerrainRequired {
            get { return _terrainRequired; }
        }
        [SerializeField] private TerrainType _terrainRequired;

        public TerrainShape ShapeRequired {
            get { return _shapeRequired; }
        }
        [SerializeField] private TerrainShape _shapeRequired;

        public TerrainFeature FeatureRequired {
            get { return _featureRequired; }
        }
        [SerializeField] private TerrainFeature _featureRequired;

        public bool MustBeUnderwater {
            get { return _mustBeUnderwater; }
        }
        [SerializeField] private bool _mustBeUnderwater;

        public ResourceSummary BonusYield {
            get { return _bonusYield; }
        }
        [SerializeField] ResourceSummary _bonusYield;

        #endregion

        #endregion

        #region constructors

        public CellYieldModificationData(TerrainType terrainRequired, ResourceSummary bonusYield) {
            _propertyConsidered = CellPropertyType.Terrain;
            _terrainRequired    = terrainRequired;
            _bonusYield         = bonusYield;

            _mustBeUnderwater = false;

            _shapeRequired    = TerrainShape.Flatlands;
            _featureRequired  = TerrainFeature.None;
            _mustBeUnderwater = false;
        }

        public CellYieldModificationData(TerrainShape shapeRequired, ResourceSummary bonusYield) {
            _propertyConsidered = CellPropertyType.Shape;
            _shapeRequired      = shapeRequired;
            _bonusYield         = bonusYield;

            _mustBeUnderwater = false;

            _terrainRequired  = TerrainType.Grassland;
            _featureRequired  = TerrainFeature.None;
            _mustBeUnderwater = false;
        }

        public CellYieldModificationData(TerrainFeature featureRequired, ResourceSummary bonusYield) {
            _propertyConsidered = CellPropertyType.Feature;
            _featureRequired    = featureRequired;
            _bonusYield         = bonusYield;

            _terrainRequired  = TerrainType.Grassland;
            _shapeRequired    = TerrainShape.Flatlands;
            _mustBeUnderwater = false;
        }

        public CellYieldModificationData(bool mustBeUnderwater, ResourceSummary bonusYield) {
            _propertyConsidered = CellPropertyType.CellIsUnderwater;
            _mustBeUnderwater   = mustBeUnderwater;
            _bonusYield         = bonusYield;

            _terrainRequired = TerrainType.Grassland;
            _shapeRequired   = TerrainShape.Flatlands;
            _featureRequired = TerrainFeature.None;
        }

        #endregion

    }

}
