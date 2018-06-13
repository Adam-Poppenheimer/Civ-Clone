using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    [CreateAssetMenu(menuName = "Civ Clone/Feature Config")]
    public class FeatureConfig : ScriptableObject, IFeatureConfig {

        #region instance fields and properties

        #region from IFeatureConfig

        public float BuildingAppearanceChance {
            get { return _buildingAppearanceChance; }
        }
        [SerializeField] private float _buildingAppearanceChance;

        public float ResourceAppearanceChance {
            get { return _resourceAppearanceChance; }
        }
        [SerializeField] private float _resourceAppearanceChance;

        public float TreeAppearanceChance {
            get { return _treeAppearanceChance; }
        }
        [SerializeField] private float _treeAppearanceChance;

        public float ImprovementAppearanceChance {
            get { return _improvementAppearanceChance; }
        }
        [SerializeField] private float _improvementAppearanceChance;

        public ReadOnlyCollection<Transform> BuildingPrefabs {
            get { return _buildingPrefabs.AsReadOnly(); }
        }
        [SerializeField] private List<Transform> _buildingPrefabs;

        public ReadOnlyCollection<Transform> ForestTreePrefabs {
            get { return _forestTreePrefabs.AsReadOnly(); }
        }
        [SerializeField] private List<Transform> _forestTreePrefabs;

        public ReadOnlyCollection<Transform> JungleTreePrefabs {
            get { return _jungleTreePrefabs.AsReadOnly(); }
        }
        [SerializeField] private List<Transform> _jungleTreePrefabs;

        public int GuaranteedTreeModulo {
            get { return _guaranteedTreeModulo; }
        }
        [SerializeField] private int _guaranteedTreeModulo;

        public int GuaranteedBuildingModulo {
            get { return _guaranteedBuildingModulo; }
        }
        [SerializeField] private int _guaranteedBuildingModulo;

        public int GuaranteedResourceModulo {
            get { return _guaranteedResourceModulo; }
        }
        [SerializeField] private int _guaranteedResourceModulo;

        public int GuaranteedImprovementModulo {
            get { return _guaranteedImprovementModulo; }
        }
        [SerializeField] private int _guaranteedImprovementModulo;

        #endregion

        #endregion

    }

}
