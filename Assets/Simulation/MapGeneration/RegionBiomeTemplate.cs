using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Map Generation/Region Biome Template")]
    public class RegionBiomeTemplate : ScriptableObject, IRegionBiomeTemplate {

        #region instance fields and properties

        #region from IRegionGenerationTemplate

        public float MinTemperature {
            get { return _minTemperature; }
        }
        [SerializeField, Range(0, 1)] private float _minTemperature;

        public float MaxTemperature {
            get { return _maxTemperature; }
        }
        [SerializeField, Range(0, 1)] private float _maxTemperature;

        public float MinPrecipitation {
            get { return _minPrecipitation; }
        }
        [SerializeField, Range(0, 1)] private float _minPrecipitation;

        public float MaxPrecipitation {
            get { return _maxPrecipitation; }
        }
        [SerializeField, Range(0, 1)] private float _maxPrecipitation;



        public int GrasslandPercentage {
            get { return _grasslandPercentage; }
        }
        [SerializeField, Range(0, 100)] private int _grasslandPercentage;

        public int PlainsPercentage {
            get { return _plainsPercentage; }
        }
        [SerializeField, Range(0, 100)] private int _plainsPercentage;

        public int DesertPercentage {
            get { return _desertPercentage; }
        }
        [SerializeField, Range(0, 100)] private int _desertPercentage;

        public int TundraPercentage {
            get { return _tundraPercentage; }
        }
        [SerializeField, Range(0, 100)] private int _tundraPercentage;

        public int SnowPercentage {
            get { return _snowPercentage; }
        }
        [SerializeField, Range(0, 100)] private int _snowPercentage;

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



        public IEnumerable<RegionBalanceStrategyData> BalanceStrategyWeights {
            get { return _balanceStrategyWeights; }
        }
        [SerializeField] private List<RegionBalanceStrategyData> _balanceStrategyWeights;

        public IEnumerable<RegionResourceData> ResourceWeights {
            get { return _resourceWeights; }
        }
        [SerializeField] private List<RegionResourceData> _resourceWeights;

        #endregion


        [SerializeField] private int TreesOnGrasslandCrawlCost;
        [SerializeField] private int TreesOnPlainsCrawlCost;
        [SerializeField] private int TreesOnTundraCrawlCost;

        [SerializeField] private int TreesOnFlatlandsCrawlCost;
        [SerializeField] private int TreesOnHillsCrawlCost;

        #endregion

        #region instance methods

        #region from IRegionGenerationTemplate

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

        #endregion

        #endregion

    }

}
