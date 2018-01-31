using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    [CreateAssetMenu(fileName = "New Hex Grid Config", menuName = "Civ Clone/Hex Grid Config")]
    public class HexGridConfig : ScriptableObject, IHexGridConfig {

        #region instance fields and properties

        #region from IHexGridConfig

        public int RandomSeed {
            get { return _randomSeed; }
        }
        [SerializeField] private int _randomSeed;

        public ReadOnlyCollection<ResourceSummary> TerrainYields {
            get {
                if(_terrainYields == null) {
                    _terrainYields = new List<ResourceSummary>() {
                        GrasslandYield, PlainsYield, DesertYield, RockYield, SnowYield
                    };
                }
                return _terrainYields.AsReadOnly();
            }
        }
        [NonSerialized] private List<ResourceSummary> _terrainYields;

        public ReadOnlyCollection<ResourceSummary> FeatureYields {
            get {
                if(_featureYields == null) {
                    _featureYields = new List<ResourceSummary>() {
                        NoFeatureYield, ForestYield
                    };
                }
                return _featureYields.AsReadOnly();
            }
        }
        [NonSerialized] private List<ResourceSummary> _featureYields;

        public int BaseLandMoveCost {
            get { return _baseLandMoveCost; }
        }
        [SerializeField] private int _baseLandMoveCost;

        public ReadOnlyCollection<int> FeatureMoveCosts {
            get {
                if(_featureMoveCosts == null) {
                    _featureMoveCosts = new List<int>() {
                        NoFeatureMoveCost, ForestMoveCost
                    };
                }
                return _featureMoveCosts.AsReadOnly();
            }
        }
        [NonSerialized] private List<int> _featureMoveCosts;

        public int WaterMoveCost {
            get { return _waterMoveCost; }
        }
        [SerializeField] private int _waterMoveCost;

        public int SlopeMoveCost {
            get { return _slopeMoveCost; }
        }
        [SerializeField] private int _slopeMoveCost;

        #endregion

        [SerializeField] private ResourceSummary GrasslandYield = ResourceSummary.Empty;
        [SerializeField] private ResourceSummary PlainsYield    = ResourceSummary.Empty;
        [SerializeField] private ResourceSummary DesertYield    = ResourceSummary.Empty;
        [SerializeField] private ResourceSummary RockYield      = ResourceSummary.Empty;
        [SerializeField] private ResourceSummary SnowYield      = ResourceSummary.Empty;

        [SerializeField] private ResourceSummary NoFeatureYield = ResourceSummary.Empty;
        [SerializeField] private ResourceSummary ForestYield    = ResourceSummary.Empty;

        [SerializeField] private int NoFeatureMoveCost = 0;
        [SerializeField] private int ForestMoveCost    = 0;

        #endregion

    }

}
