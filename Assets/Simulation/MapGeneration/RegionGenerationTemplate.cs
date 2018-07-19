using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Region Generation Template")]
    public class RegionGenerationTemplate : ScriptableObject, IRegionGenerationTemplate {

        #region instance fields and properties

        #region from IRegionGenerationTemplate

        public int HillsPercentage {
            get { return _hillsPercentage; }
        }
        [SerializeField, Range(0, 100)] private int _hillsPercentage;

        public int MountainsPercentage {
            get { return _mountainsPercentage; }
        }
        [SerializeField, Range(0, 100)] private int _mountainsPercentage;

        public int TreePercentage {
            get { return _treePercentage; }
        }
        [SerializeField, Range(0, 100)] private int _treePercentage;


        public int MinTreeClumps {
            get { return _minTreeClumps; }
        }
        [SerializeField, Range(0, 20)] private int _minTreeClumps = 2;

        public int MaxTreeClumps {
            get { return _maxTreeClumps; }
        }
        [SerializeField, Range(0, 20)] private int _maxTreeClumps = 3;

        public float JungleThreshold {
            get { return _jungleThreshold; }
        }
        [SerializeField, Range(0f, 1f)] private float _jungleThreshold = 0.75f;



        public float MarshChanceBase {
            get { return _marshChanceBase; }
        }
        [SerializeField, Range(0f, 1f)] private float _marshChanceBase = 0.05f;

        public float MarshChancePerAdjacentWater {
            get { return _marshChancePerAdjacentWater; }
        }
        [SerializeField, Range(0f, 1f)] private float _marshChancePerAdjacentWater = 0.05f;

        public float MarshChancePerAdjacentRiver {
            get { return _marshChancePerAdjacentRiver; }
        }
        [SerializeField, Range(0f, 1f)] private float _marshChancePerAdjacentRiver = 0.05f;

        #endregion

        [SerializeField, Range(0, 100)] private int GrasslandsPercentage;
        [SerializeField, Range(0, 100)] private int PlainsPercentage;
        [SerializeField, Range(0, 100)] private int DesertPercentage;
        [SerializeField, Range(0, 100)] private int TundraPercentage;
        [SerializeField, Range(0, 100)] private int SnowPercentage;        

        [SerializeField, Range(0, 20)] private int GrasslandSeedCount;
        [SerializeField, Range(0, 20)] private int PlainsSeedCount;
        [SerializeField, Range(0, 20)] private int DesertSeedCount;
        [SerializeField, Range(0, 20)] private int TundraSeedCount;
        [SerializeField, Range(0, 20)] private int SnowSeedCount;

        [SerializeField, Range(0f, 1f)] private float TundraThreshold;
        [SerializeField, Range(0f, 1f)] private float SnowThreshold;
        

        private TerrainData GrasslandData {
            get {
                if(_grasslandData == null) {
                    _grasslandData = new TerrainData(
                        GrasslandsPercentage, GrasslandSeedCount,
                        DefaultWeightFunction, AutoRejectThresholdFunction
                    );
                }

                return _grasslandData;
            }
        }
        private TerrainData _grasslandData;

        private TerrainData PlainsData {
            get {
                if(_plainsData == null) {
                    _plainsData = new TerrainData(
                        PlainsPercentage, PlainsSeedCount,
                        DefaultWeightFunction, AutoRejectThresholdFunction
                    );
                }

                return _plainsData;
            }
        }
        private TerrainData _plainsData;

        private TerrainData DesertData {
            get {
                if(_desertData == null) {
                    _desertData = new TerrainData(
                        DesertPercentage, DesertSeedCount,
                        DefaultWeightFunction, AutoRejectThresholdFunction
                    );
                }

                return _desertData;
            }
        }
        private TerrainData _desertData;

        private TerrainData TundraData {
            get {
                if(_tundraData == null) {
                    var thresholdFunction = BuildTemperatureThresholdFunction(
                        SnowThreshold, TundraThreshold, UnityEngine.Random.Range(0, 4)
                    );

                    _tundraData = new TerrainData(
                        TundraPercentage, TundraSeedCount,
                        DefaultWeightFunction, thresholdFunction
                    );
                }

                return _tundraData;
            }
        }
        private TerrainData _tundraData;

        private TerrainData SnowData {
            get {
                if(_snowData == null) {
                    var thresholdFunction = BuildTemperatureThresholdFunction(
                        0, SnowThreshold, UnityEngine.Random.Range(0, 4)
                    );

                    _snowData = new TerrainData(
                        SnowPercentage, SnowSeedCount,
                        DefaultWeightFunction, thresholdFunction
                    );
                }

                return _snowData;
            }
        }
        private TerrainData _snowData;

        private TerrainData DefaultData {
            get {
                if(_defaultData == null) {
                    _defaultData = new TerrainData(
                        0, 0, DefaultWeightFunction, AutoRejectThresholdFunction
                    );
                }

                return _defaultData;
            }
        }
        private TerrainData _defaultData;



        private ICellTemperatureLogic TemperatureLogic;
        private IHexGrid              Grid;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICellTemperatureLogic temperatureLogic, IHexGrid grid
        ) {
            TemperatureLogic = temperatureLogic;
            Grid             = grid;
        }

        #region from IRegionGenerationTemplate

        public TerrainData GetTerrainData(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.Grassland: return GrasslandData;
                case CellTerrain.Plains:    return PlainsData;
                case CellTerrain.DeepWater: return DesertData;
                case CellTerrain.Tundra:    return TundraData;
                case CellTerrain.Snow:      return SnowData;

                default: return DefaultData;
            }
        }

        #endregion

        private bool AutoRejectThresholdFunction(IHexCell cell) {
            return false;
        }

        private Predicate<IHexCell> BuildTemperatureThresholdFunction(
            float minTemperature, float maxTemperature, int jitterChannel
        ) {
            return delegate(IHexCell cell) {
                float temperature = TemperatureLogic.GetTemperatureOfCell(cell, jitterChannel);

                return temperature >= minTemperature && temperature < maxTemperature;
            };
        }

        private int DefaultWeightFunction(IHexCell cell, IHexCell seed) {
            return Grid.GetDistance(cell, seed);
        }

        #endregion

    }

}
