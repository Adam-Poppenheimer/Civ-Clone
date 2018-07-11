using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

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


        public int MapBorderX {
            get { return _mapBorderX; }
        }
        [SerializeField, Range(0, 10)] private int _mapBorderX;

        public int MapBorderZ {
            get { return _mapBorderZ; }
        }
        [SerializeField, Range(0, 10)] private int _mapBorderZ;

        public int RegionBorder {
            get { return _regionBorder; }
        }
        [SerializeField, Range(0, 10)] private int _regionBorder;

        public int RegionCount {
            get { return _regionCount; }
        }
        [SerializeField, Range(1, 4)] private int _regionCount = 1;

        #endregion

        #endregion

    }

}
