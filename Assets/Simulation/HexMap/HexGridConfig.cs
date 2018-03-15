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
            get { return _terrainYields.AsReadOnly(); }
        }
        [SerializeField] private List<ResourceSummary> _terrainYields;

        public ReadOnlyCollection<ResourceSummary> FeatureYields {
            get { return _featureYields.AsReadOnly(); }
        }
        [SerializeField] private List<ResourceSummary> _featureYields;

        public ReadOnlyCollection<ResourceSummary> ShapeYields {
            get { return _shapeYields.AsReadOnly(); }
        }
        [SerializeField] private List<ResourceSummary> _shapeYields;

        public int BaseLandMoveCost {
            get { return _baseLandMoveCost; }
        }
        [SerializeField] private int _baseLandMoveCost;

        public ReadOnlyCollection<int> FeatureMoveCosts {
            get { return _featureMoveCosts.AsReadOnly(); }
        }
        [SerializeField] private List<int> _featureMoveCosts;

        public ReadOnlyCollection<int> ShapeMoveCosts {
            get { return _shapeMoveCosts.AsReadOnly(); }
        }
        [SerializeField] private List<int> _shapeMoveCosts;

        public int WaterMoveCost {
            get { return _waterMoveCost; }
        }
        [SerializeField] private int _waterMoveCost;

        public int SlopeMoveCost {
            get { return _slopeMoveCost; }
        }
        [SerializeField] private int _slopeMoveCost;

        public float RoadMoveCostMultiplier {
            get { return _roadMoveCostMultiplier; }
        }
        [SerializeField] private float _roadMoveCostMultiplier;

        #endregion

        #endregion

    }

}
