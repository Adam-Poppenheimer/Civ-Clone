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

        public int RiverPercentage {
            get { return _riverPercentage; }
        }
        [SerializeField, Range(0, 100)] private int _riverPercentage;


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



        public bool HasPrimaryLuxury {
            get { return _hasLuxuryResource; }
        }
        [SerializeField] private bool _hasLuxuryResource;

        public int PrimaryLuxuryCount {
            get { return _primaryLuxuryCount; }
        }
        [SerializeField, Range(0, 10)] private int _primaryLuxuryCount;

        public bool HasSecondaryLuxury {
            get { return _hasSecondaryLuxury; }
        }
        [SerializeField] private bool _hasSecondaryLuxury;

        public int SecondaryLuxuryCount {
            get { return _secondaryLuxuryCount; }
        }
        [SerializeField, Range(0, 10)] private int _secondaryLuxuryCount;

        public bool HasTertiaryLuxury {
            get { return _hasTertiaryLuxury; }
        }
        [SerializeField] private bool _hasTertiaryLuxury;

        public int TertiaryLuxuryCount {
            get { return _tertiaryLuxuryCount; }
        }
        [SerializeField, Range(0, 10)] private int _tertiaryLuxuryCount;

        public bool HasQuaternaryLuxury {
            get { return _hasQuaternaryLuxury; }
        }
        [SerializeField] private bool _hasQuaternaryLuxury;

        public int QuaternaryLuxuryCount {
            get { return _quaternaryLuxuryCount; }
        }
        [SerializeField, Range(0, 10)] private int _quaternaryLuxuryCount;

        public float StrategicNodesPerCell {
            get { return _strategicNodesPerCell; }
        }
        [SerializeField, Range(0f, 0.5f)] private float _strategicNodesPerCell;

        public float StrategicCopiesPerCell {
            get { return _strategicCopiesPerCell; }
        }
        [SerializeField, Range(0f, 2f)] private float _strategicCopiesPerCell;



        public bool BalanceResources {
            get { return _balanceResources; }
        }
        [SerializeField] private bool _balanceResources;

        public float MinFoodPerCell {
            get { return _minFoodPerCell; }
        }
        [SerializeField, Range(0.5f, 4f)] private float _minFoodPerCell = 0;

        public float MinProductionPerCell {
            get { return _minProductionPerCell; }
        }
        [SerializeField, Range(0.5f, 4f)] private float _minProductionPerCell = 0;


        public float MinScorePerCell {
            get { return _minScorePerCell; }
        }
        [SerializeField, Range(1f, 10f)] private float _minScorePerCell = 0;

        public float MaxScorePerCell {
            get { return _maxScorePerCell; }
        }
        [SerializeField, Range(1f, 10f)] private float _maxScorePerCell = 0;

        #endregion

        [SerializeField, Range(0, 100)] private int GrasslandsPercentage;
        [SerializeField, Range(0, 100)] private int PlainsPercentage;
        [SerializeField, Range(0, 100)] private int DesertPercentage;
        [SerializeField, Range(0, 100)] private int TundraPercentage;
        [SerializeField, Range(0, 100)] private int SnowPercentage;
        [SerializeField, Range(0, 100)] private int WaterPercentage;

        [SerializeField, Range(0, 20)] private int GrasslandSeedCount;
        [SerializeField, Range(0, 20)] private int PlainsSeedCount;
        [SerializeField, Range(0, 20)] private int DesertSeedCount;
        [SerializeField, Range(0, 20)] private int TundraSeedCount;
        [SerializeField, Range(0, 20)] private int SnowSeedCount;
        [SerializeField, Range(0, 20)] private int WaterSeedCount;

        [SerializeField, Range(0f, 1f)] private float TundraThreshold;
        [SerializeField, Range(0f, 1f)] private float SnowThreshold;
        

        private TerrainData GrasslandData {
            get {
                if(grasslandData == null) {
                    grasslandData = new TerrainData(
                        GrasslandsPercentage, GrasslandSeedCount,
                        DefaultWeightFunction, DefaultSeedFilter,
                        AutoRejectThresholdFunction
                    );
                }

                return grasslandData;
            }
        }
        private TerrainData grasslandData;

        private TerrainData PlainsData {
            get {
                if(plainsData == null) {
                    plainsData = new TerrainData(
                        PlainsPercentage, PlainsSeedCount,
                        DefaultWeightFunction, DefaultSeedFilter,
                        AutoRejectThresholdFunction
                    );
                }

                return plainsData;
            }
        }
        private TerrainData plainsData;

        private TerrainData DesertData {
            get {
                if(desertData == null) {
                    desertData = new TerrainData(
                        DesertPercentage, DesertSeedCount,
                        DefaultWeightFunction, DefaultSeedFilter,
                        AutoRejectThresholdFunction
                    );
                }

                return desertData;
            }
        }
        private TerrainData desertData;

        private TerrainData TundraData {
            get {
                if(tundraData == null) {
                    var thresholdFunction = BuildTemperatureThresholdFunction(
                        SnowThreshold, TundraThreshold, UnityEngine.Random.Range(0, 4)
                    );

                    tundraData = new TerrainData(
                        TundraPercentage, TundraSeedCount,
                        DefaultWeightFunction, DefaultSeedFilter,
                        thresholdFunction
                    );
                }

                return tundraData;
            }
        }
        private TerrainData tundraData;

        private TerrainData SnowData {
            get {
                if(snowData == null) {
                    var thresholdFunction = BuildTemperatureThresholdFunction(
                        0, SnowThreshold, UnityEngine.Random.Range(0, 4)
                    );

                    snowData = new TerrainData(
                        SnowPercentage, SnowSeedCount,
                        DefaultWeightFunction, DefaultSeedFilter,
                        thresholdFunction
                    );
                }

                return snowData;
            }
        }
        private TerrainData snowData;

        private TerrainData WaterData {
            get {
                if(waterData == null) {
                    waterData = new TerrainData(
                        WaterPercentage, WaterSeedCount,
                        DefaultWeightFunction, WaterSeedFilter,
                        AutoRejectThresholdFunction
                    );
                }

                return waterData;
            }
        }
        private TerrainData waterData;

        private TerrainData DefaultData {
            get {
                if(defaultData == null) {
                    defaultData = new TerrainData(
                        0, 0, DefaultWeightFunction, DefaultSeedFilter,
                        AutoRejectThresholdFunction
                    );
                }

                return defaultData;
            }
        }
        private TerrainData defaultData;



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

        #region Unity messages

        private void OnValidate() {
            grasslandData = null;
            plainsData    = null;
            desertData    = null;
            tundraData    = null;
            snowData      = null;
            waterData     = null;
            defaultData   = null;
        }

        #endregion

        #region from IRegionGenerationTemplate

        public TerrainData GetTerrainData(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.Grassland: return GrasslandData;
                case CellTerrain.Plains:    return PlainsData;
                case CellTerrain.Desert:    return DesertData;
                case CellTerrain.Tundra:    return TundraData;
                case CellTerrain.Snow:      return SnowData;
                case CellTerrain.DeepWater: return WaterData;

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

        private int DefaultWeightFunction(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
            return Grid.GetDistance(cell, seed);
        }

        private int WaterWeightFunction(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
            int distance = Grid.GetDistance(cell, seed);

            if(Grid.GetNeighbors(cell).Exists(
                neighbor => neighbor.Terrain.IsWater() || acceptedCells.Contains(neighbor)
            )) {
                return distance;
            }else {
                return distance + 100;
            }
        }

        private bool DefaultSeedFilter(IHexCell cell, IEnumerable<IHexCell> oceanCells) {
            return true;
        }

        private bool WaterSeedFilter(IHexCell cell, IEnumerable<IHexCell> oceanCells) {
            return Grid.GetCellsInRadius(cell, 2).Intersect(oceanCells).Any();
        }

        #endregion

    }

}
