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

        public CellTerrain TerrainRequired {
            get { return _terrainRequired; }
        }
        [SerializeField] private CellTerrain _terrainRequired;

        public CellShape ShapeRequired {
            get { return _shapeRequired; }
        }
        [SerializeField] private CellShape _shapeRequired;

        public CellVegetation VegetationRequired {
            get { return _vegetationRequired; }
        }
        [SerializeField] private CellVegetation _vegetationRequired;

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

        public CellYieldModificationData(CellTerrain terrainRequired, ResourceSummary bonusYield) {
            _propertyConsidered = CellPropertyType.Terrain;
            _terrainRequired    = terrainRequired;
            _bonusYield         = bonusYield;

            _mustBeUnderwater = false;

            _shapeRequired    = CellShape.Flatlands;
            _vegetationRequired  = CellVegetation.None;
            _mustBeUnderwater = false;
        }

        public CellYieldModificationData(CellShape shapeRequired, ResourceSummary bonusYield) {
            _propertyConsidered = CellPropertyType.Shape;
            _shapeRequired      = shapeRequired;
            _bonusYield         = bonusYield;

            _mustBeUnderwater = false;

            _terrainRequired  = CellTerrain.Grassland;
            _vegetationRequired  = CellVegetation.None;
            _mustBeUnderwater = false;
        }

        public CellYieldModificationData(CellVegetation featureRequired, ResourceSummary bonusYield) {
            _propertyConsidered = CellPropertyType.Vegetation;
            _vegetationRequired    = featureRequired;
            _bonusYield         = bonusYield;

            _terrainRequired  = CellTerrain.Grassland;
            _shapeRequired    = CellShape.Flatlands;
            _mustBeUnderwater = false;
        }

        public CellYieldModificationData(bool mustBeUnderwater, ResourceSummary bonusYield) {
            _propertyConsidered = CellPropertyType.CellIsUnderwater;
            _mustBeUnderwater   = mustBeUnderwater;
            _bonusYield         = bonusYield;

            _terrainRequired = CellTerrain.Grassland;
            _shapeRequired   = CellShape.Flatlands;
            _vegetationRequired = CellVegetation.None;
        }

        #endregion

    }

}
