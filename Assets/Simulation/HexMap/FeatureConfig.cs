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
            get { return _improvementAppearanceChance; }
        }
        [SerializeField] private float _improvementAppearanceChance;

        public float TreeAppearanceChance {
            get { return _treeAppearanceChance; }
        }
        [SerializeField] private float _treeAppearanceChance;

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

        #endregion

        #endregion

    }

}
