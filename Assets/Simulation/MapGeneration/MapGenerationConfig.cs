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

        public float JitterProbability {
            get { return _jitterProbability; }
        }
        [SerializeField, Range(0f, 0.5f)] private float _jitterProbability;

        public int SectionSizeMin {
            get { return _chunkSizeMin; }
        }
        [SerializeField, Range(20, 200)] private int _chunkSizeMin;

        public int SectionSizeMax {
            get { return _chunkSizeMax; }
        }
        [SerializeField, Range(20, 200)] private int _chunkSizeMax;

        public int LandPercentage {
            get { return _landPercentage; }
        }
        [SerializeField, Range(5, 95)] private int _landPercentage = 50;

        public float MountainThreshold {
            get { return _mountainThreshold; }
        }
        [SerializeField] private float _mountainThreshold;

        public float HillsThreshold {
            get { return _hillsThreshold; }
        }
        [SerializeField] private float _hillsThreshold;

        public float FlatlandsThreshold {
            get { return _flatlandsThreshold; }
        }
        [SerializeField] private float _flatlandsThreshold;

        public float SectionRaisePressureMin {
            get { return _sectionRaisePressureMin; }
        }
        [SerializeField] private float _sectionRaisePressureMin;

        public float SectionRaisePressureMax {
            get { return _sectionRaisePressureMax; }
        }
        [SerializeField] private float _sectionRaisePressureMax;

        public int LakeMaxSize {
            get { return _lakeMaxSize; }
        }
        [SerializeField] private int _lakeMaxSize;

        public int InlandSeaMaxSize {
            get { return _inlandSeaMaxSize; }
        }
        [SerializeField] private int _inlandSeaMaxSize;

        public int ContinentalShelfDistance {
            get { return _continentalShelfDistance; }
        }
        [SerializeField] private int _continentalShelfDistance;


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

        public int SoftBorderAvoidance {
            get { return _softBorderAvoidance; }
        }
        [SerializeField, Range(1, 100)] private int _softBorderAvoidance = 10;


        public int RegionBorder {
            get { return _regionBorder; }
        }
        [SerializeField, Range(0, 10)] private int _regionBorder;

        public int RegionCount {
            get { return _regionCount; }
        }
        [SerializeField, Range(1, 4)] private int _regionCount = 1;



        public float StartingMoisture {
            get { return _startingMoisture; }
        }
        [SerializeField, Range(0f, 1f)] private float _startingMoisture = 0.1f;

        public float EvaporationCoefficient {
            get { return _evaporationCoefficient; }
        }
        [SerializeField, Range(0f, 1f)] private float _evaporationCoefficient = 0.5f;

        public float PrecipitationCoefficient {
            get { return _precipitationCoefficient; }
        }
        [SerializeField, Range(0f, 1f)] private float _precipitationCoefficient = 0.25f;

        public float RunoffCoefficient {
            get { return _runoffCoefficient; }
        }
        [SerializeField, Range(0f, 1f)] private float _runoffCoefficient = 0.25f;

        public float SeepageCoefficient {
            get { return _seepageCoefficient; }
        }
        [SerializeField, Range(0f, 1f)] private float _seepageCoefficient = 0.125f;


        public HexDirection WindDirection {
            get { return _windDirection; }
        }
        [SerializeField] private HexDirection _windDirection = HexDirection.NW;

        public float WindStrength {
            get { return _windStrength; }
        }
        [SerializeField, Range(1f, 10f)] private float _windStrength = 4f;


        public float FlatlandsCloudMaximum {
            get { return _flatlandsCloudMaximum; }
        }
        [SerializeField, Range(0f, 1f)] private float _flatlandsCloudMaximum = 1f;

        public float HillsCloudMaximum {
            get { return _hillsCloudMaximum; }
        }
        [SerializeField, Range(0f, 1f)] private float _hillsCloudMaximum = 0.5f;

        public float MountainsCloudMaximum {
            get { return _mountainsCloudMaximum; }
        }
        [SerializeField, Range(0f, 1f)] private float _mountainsCloudMaximum = 0.1f;



        public int RiverSegmentPercentage {
            get { return _riverSegmentPercentage; }
        }
        [SerializeField, Range(0, 20)] private int _riverSegmentPercentage = 10;

        public int RiverMaxLengthInHexes {
            get { return _riverMaxLengthInHexes; }
        }
        [SerializeField, Range(0, 100)] private int _riverMaxLengthInHexes = 30;



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

        public float TemperatureJitter {
            get { return _temperatureJitter; }
        }
        [SerializeField, Range(0f, 1f)] private float _temperatureJitter = 0.1f;

        public float CoastChancePerAdjacentOcean {
            get { return _coastChancePerAdjacentOcean; }
        }
        [SerializeField, Range(0f, 1f)] private float _coastChancePerAdjacentOcean = 0.45f;

        public float CoastChancePerNearbyOcean {
            get { return _coastChancePerNearbyOcean; }
        }
        [SerializeField, Range(0f, 0.1f)] private float _coastChancePerNearbyOcean = 0.025f;




        public IMapGenerationTemplate MapTemplate {
            get { return _mapTemplate; }
        }
        [SerializeField] private MapGenerationTemplate _mapTemplate;

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

        #endregion

        #endregion

    }

}
