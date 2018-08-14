﻿using System;
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

        public float TemperatureJitter {
            get { return _temperatureJitter; }
        }
        [SerializeField, Range(0f, 1f)] private float _temperatureJitter = 0.1f;




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



        public int MaxLakeSize {
            get { return _maxLakeSize; }
        }
        [SerializeField] private int _maxLakeSize;

        #endregion

        #endregion

    }

}
