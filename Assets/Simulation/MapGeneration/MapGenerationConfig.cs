using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Map Generation Config")]
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


        public int HardMapBorderX {
            get { return _hardMapBorderX; }
        }
        [SerializeField, Range(0, 10)] private int _hardMapBorderX = 2;

        public int HardMapBorderZ {
            get { return _hardMapBorderZ; }
        }
        [SerializeField, Range(0, 10)] private int _hardMapBorderZ = 2;

        public int SoftMapBorderX {
            get { return _softMapBorderX; }
        }
        [SerializeField, Range(0, 10)] private int _softMapBorderX = 2;

        public int SoftMapBorderZ {
            get { return _softMapBorderZ; }
        }
        [SerializeField, Range(0, 10)] private int _softMapBorderZ = 2;

        public int SoftBorderAvoidanceWeight {
            get { return _softBorderAvoidanceWeight; }
        }
        [SerializeField] private int _softBorderAvoidanceWeight = 10;



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




        public IMapTemplate MapTemplate {
            get { return _mapTemplate; }
        }
        [SerializeField] private MapTemplate _mapTemplate;

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

        #endregion

        [SerializeField, Range(0f, 1f)] private float IdealGrasslandTemperature;
        [SerializeField, Range(0f, 1f)] private float IdealPlainsTemperature;
        [SerializeField, Range(0f, 1f)] private float IdealDesertTemperature;

        [SerializeField, Range(0f, 1f)] private float IdealGrasslandPrecipitation;
        [SerializeField, Range(0f, 1f)] private float IdealPlainsPrecipitation;
        [SerializeField, Range(0f, 1f)] private float IdealDesertPrecipitation;

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

        #endregion

        #endregion

    }

}
