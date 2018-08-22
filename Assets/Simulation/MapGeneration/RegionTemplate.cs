using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Region Generation Template")]
    public class RegionTemplate : ScriptableObject, IRegionTemplate {

        #region internal types

        [Serializable]
        public struct BalanceStrategySummary {
            
            public string Strategy {
                get { return _strategy; }
            }
            [SerializeField] private string _strategy;

            public int Weight {
                get { return _weight; }
            }
            [SerializeField] private int _weight;

        }

        [Serializable]
        public struct MapResourceSummary {

            public ResourceDefinition Resource;

            public int Weight;

        }

        #endregion

        #region instance fields and properties

        #region from IRegionGenerationTemplate

        public int TundraPercentage {
            get { return _tundraPercentage; }
        }

        public int SnowPercentage {
            get { return _snowPercentage; }
        }

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

        public bool AreTreesJungle {
            get { return _areTreesJungle; }
        }
        [SerializeField] private bool _areTreesJungle;



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
        [SerializeField, Range(0, 100)] private int _tundraPercentage;
        [SerializeField, Range(0, 100)] private int _snowPercentage;

        [SerializeField, Range(0, 20)] private int GrasslandSeedCount;
        [SerializeField, Range(0, 20)] private int PlainsSeedCount;
        [SerializeField, Range(0, 20)] private int DesertSeedCount;


        [SerializeField] private int TreesOnGrasslandCrawlCost;
        [SerializeField] private int TreesOnPlainsCrawlCost;
        [SerializeField] private int TreesOnTundraCrawlCost;

        [SerializeField] private int TreesOnFlatlandsCrawlCost;
        [SerializeField] private int TreesOnHillsCrawlCost;

        private TerrainData GrasslandData {
            get {
                if(grasslandData == null) {
                    grasslandData = new TerrainData(
                        GrasslandsPercentage, GrasslandSeedCount,
                        DefaultCrawlingWeightFunction, DefaultSeedFilter,
                        DefaultSeedWeightFunction
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
                        DefaultCrawlingWeightFunction, DefaultSeedFilter,
                        DefaultSeedWeightFunction
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
                        DefaultCrawlingWeightFunction, DefaultSeedFilter,
                        DefaultSeedWeightFunction
                    );
                }

                return desertData;
            }
        }
        private TerrainData desertData;

        private TerrainData DefaultData {
            get {
                if(defaultData == null) {
                    defaultData = new TerrainData(
                        0, 0, DefaultCrawlingWeightFunction, DefaultSeedFilter,
                        DefaultSeedWeightFunction
                    );
                }

                return defaultData;
            }
        }
        private TerrainData defaultData;

        [SerializeField] private List<BalanceStrategySummary> BalanceStrategyData;
        private Dictionary<IBalanceStrategy, int> BalanceStrategyWeights;

        [SerializeField] private List<MapResourceSummary> MapResourceData; 
        private Dictionary<IResourceDefinition, int> SelectionWeightsOfResources;



        private ICellTemperatureLogic  TemperatureLogic;
        private IHexGrid               Grid;
        private List<IBalanceStrategy> AllBalanceStrategies;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICellTemperatureLogic temperatureLogic, IHexGrid grid,
            List<IBalanceStrategy> allBalanceStrategies
        ) {
            TemperatureLogic     = temperatureLogic;
            Grid                 = grid;
            AllBalanceStrategies = allBalanceStrategies;
        }

        #region Unity messages

        private void OnValidate() {
            grasslandData = null;
            plainsData    = null;
            desertData    = null;
            defaultData   = null;

            BalanceStrategyWeights = null;
        }

        #endregion

        #region from IRegionGenerationTemplate

        public TerrainData GetNonArcticTerrainData(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.Grassland: return GrasslandData;
                case CellTerrain.Plains:    return PlainsData;
                case CellTerrain.Desert:    return DesertData;

                default: return DefaultData;
            }
        }

        public int GetWeightForBalanceStrategy(IBalanceStrategy strategyToCheck) {
            if(BalanceStrategyWeights == null) {
                BalanceStrategyWeights = new Dictionary<IBalanceStrategy, int>();

                foreach(var data in BalanceStrategyData) {
                    var strategyWithName = AllBalanceStrategies.Where(
                        strategy => strategy.Name.Equals(data.Strategy)
                    ).FirstOrDefault();

                    if(strategyWithName != null) {
                        BalanceStrategyWeights[strategyWithName] = data.Weight;
                    }
                }

                foreach(var unweightedStrategy in AllBalanceStrategies.Except(BalanceStrategyWeights.Keys).ToArray()) {
                    BalanceStrategyWeights[unweightedStrategy] = 0;
                }
            }

            return BalanceStrategyWeights[strategyToCheck];
        }

        public int GetTreePlacementCostForTerrain(CellTerrain terrain) {
            switch(terrain) {
                    case CellTerrain.Grassland: return TreesOnGrasslandCrawlCost;
                    case CellTerrain.Plains:    return TreesOnPlainsCrawlCost;
                    case CellTerrain.Tundra:    return TreesOnTundraCrawlCost;
                    default: return -1000;
                }
        }

        public int GetTreePlacementCostForShape(CellShape shape) {
            switch(shape) {
                    case CellShape.Flatlands: return TreesOnFlatlandsCrawlCost;
                    case CellShape.Hills:     return TreesOnHillsCrawlCost;
                    default: return -1000;
                }
        }

        public int GetSelectionWeightOfResource(IResourceDefinition resource) {
            if(SelectionWeightsOfResources == null) {
                SelectionWeightsOfResources = new Dictionary<IResourceDefinition, int>();

                foreach(var data in MapResourceData) {
                    SelectionWeightsOfResources[data.Resource] = data.Weight;
                }
            }

            int retval;

            SelectionWeightsOfResources.TryGetValue(resource, out retval);
            return retval;
        }

        #endregion

        private int DefaultCrawlingWeightFunction(
            IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells
        ) {
            if(acceptedCells.Contains(cell)) {
                return 0;
            }else {
                return Grid.GetDistance(cell, seed);
            }
        }

        private bool DefaultSeedFilter(
            IHexCell cell, IEnumerable<IHexCell> landCells, IEnumerable<IHexCell> waterCells
        ) {
            return true;
        }

        private int DefaultSeedWeightFunction(IHexCell cell) {
            return 1;
        }

        #endregion

    }

}
