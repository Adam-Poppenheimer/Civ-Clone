using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Map Generation/Config")]
    public class MapGenerationConfig : ScriptableObject, IMapGenerationConfig {

        #region instance fields and properties

        #region from IMapGenerationConfig

        public bool UseFixedSeed {
            get { return _useFixedSeed; }
        }
        [SerializeField] private bool _useFixedSeed;

        public int FixedSeed {
            get { return _fixedSeed; }
        }
        [SerializeField] private int _fixedSeed;

        public int LandPercentage {
            get { return _landPercentage; }
        }
        [SerializeField, Range(5, 95)] private int _landPercentage = 50;



        public float LowTemperature {
            get { return _lowTemperature; }
        }
        [SerializeField, Range(0f, 1f)] private float _lowTemperature = 0f;

        public float HighTemperature {
            get { return _highTemperature; }
        }
        [SerializeField, Range(0f, 1f)] private float _highTemperature = 1f;

        public HemisphereMode Hemispheres {
            get { return _hemispheres; }
        }
        [SerializeField] private HemisphereMode _hemispheres = HemisphereMode.Both;




        public IMapTemplate TestTemplate {
            get { return _testTemplate; }
        }
        [SerializeField] private MapTemplate _testTemplate;

        public int MinStrategicCopies {
            get { return _minStrategicCopies; }
        }
        [SerializeField, Range(0, 20)] private int _minStrategicCopies = 2;

        public int MaxStrategicCopies {
            get { return _maxStrategicCopies; }
        }
        [SerializeField, Range(0, 20)] private int _maxStrategicCopies = 12;


        public YieldSummary YieldScoringWeights {
            get { return _yieldScoringWeights; }
        }
        [SerializeField] private YieldSummary _yieldScoringWeights;



        public int MaxLakeSize {
            get { return _maxLakeSize; }
        }
        [SerializeField] private int _maxLakeSize;



        public int BaseTerrainWeight {
            get { return _baseTerrainWeight; }
        }
        [SerializeField] private int _baseTerrainWeight;

        public int TerrainTemperatureWeight {
            get { return _terrainTemperatureWeight; }
        }
        [SerializeField] private int _terrainTemperatureWeight;

        public int TerrainPrecipitationWeight {
            get { return _terrainPrecipitationWeight; }
        }
        [SerializeField] private int _terrainPrecipitationWeight;



        public int RiverEndpointOnDesertWeight {
            get { return _riverEndpointOnDesertWeight; }
        }
        [SerializeField, Range(1, 100)] private int _riverEndpointOnDesertWeight = 50;

        public int RiverEndpointOnArcticWeight {
            get { return _riverEndpointOnArcticWeight; }
        }
        [SerializeField, Range(1, 100)] private int _riverEndpointOnArcticWeight = 50;



        public ReadOnlyCollection<IMapSizeCategory> MapSizes {
            get {
                if(_castMapSizes == null) {
                    _castMapSizes = _mapSizes.Cast<IMapSizeCategory>().ToList().AsReadOnly();
                }
                return _castMapSizes;
            }
        }
        private ReadOnlyCollection<IMapSizeCategory> _castMapSizes;
        [SerializeField] private List<MapSizeCategory> _mapSizes;

        public IMapSizeCategory DefaultMapSize {
            get { return _defaultMapSize; }
        }
        [SerializeField] private MapSizeCategory _defaultMapSize;

        #endregion

        [SerializeField, Range(0f, 1f)] private float IdealGrasslandTemperature;
        [SerializeField, Range(0f, 1f)] private float IdealPlainsTemperature;
        [SerializeField, Range(0f, 1f)] private float IdealDesertTemperature;

        [SerializeField, Range(0f, 1f)] private float IdealGrasslandPrecipitation;
        [SerializeField, Range(0f, 1f)] private float IdealPlainsPrecipitation;
        [SerializeField, Range(0f, 1f)] private float IdealDesertPrecipitation;

        [SerializeField, Range(10, 100)] private int LowSeaLevelLandPercentage;
        [SerializeField, Range(10, 100)] private int NormalSeaLevelLandPercentage;
        [SerializeField, Range(10, 100)] private int HighSeaLevelLandPercentage;

        #endregion

        #region instance methods

        #region from IMapGenerationConfig

        public float GetIdealTemperatureForTerrain(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.Grassland: return IdealGrasslandTemperature;
                case CellTerrain.Plains:    return IdealPlainsTemperature;
                case CellTerrain.Desert:    return IdealDesertTemperature;
                default: throw new NotImplementedException("No ideal temperature for terrain " + terrain);
            }
        }

        public float GetIdealPrecipitationForTerrain(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.Grassland: return IdealGrasslandPrecipitation;
                case CellTerrain.Plains:    return IdealPlainsPrecipitation;
                case CellTerrain.Desert:    return IdealDesertPrecipitation;
                default: throw new NotImplementedException("No ideal precipitation for terrain " + terrain);
            }
        }

        public int GetLandPercentageForSeaLevel(SeaLevelCategory size) {
            switch(size) {
                case SeaLevelCategory.Low:    return LowSeaLevelLandPercentage;
                case SeaLevelCategory.Normal: return NormalSeaLevelLandPercentage;
                case SeaLevelCategory.High:   return HighSeaLevelLandPercentage;
                default: Debug.LogWarningFormat("Unsupported sea level {0}. Defaulting to normal", size); return NormalSeaLevelLandPercentage;
            }
        }

        #endregion

        #endregion

    }

}
