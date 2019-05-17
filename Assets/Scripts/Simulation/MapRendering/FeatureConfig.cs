using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    [CreateAssetMenu(menuName = "Civ Clone/Map Rendering/Config")]
    public class FeatureConfig : ScriptableObject, IFeatureConfig {

        #region instance fields and properties

        #region from IFeatureConfig

        public ReadOnlyCollection<Transform> ForestTreePrefabs {
            get { return _forestTreePrefabs.AsReadOnly(); }
        }
        [SerializeField] private List<Transform> _forestTreePrefabs = null;

        public ReadOnlyCollection<Transform> JungleTreePrefabs {
            get { return _jungleTreePrefabs.AsReadOnly(); }
        }
        [SerializeField] private List<Transform> _jungleTreePrefabs = null;

        public ReadOnlyCollection<Transform> BuildingPrefabs {
            get { return _buildingPrefabs.AsReadOnly(); }
        }
        [SerializeField] private List<Transform> _buildingPrefabs = null;

        public ReadOnlyCollection<Transform> RuinsPrefabs {
            get { return _ruinsPrefabs.AsReadOnly(); }
        }
        [SerializeField] private List<Transform> _ruinsPrefabs = null;

        public ReadOnlyCollection<Transform> EncampmentPrefabs {
            get { return _encampmentPrefabs.AsReadOnly(); }
        }
        [SerializeField] private List<Transform> _encampmentPrefabs = null;





        public float TreeAppearanceChance {
            get { return _treeAppearanceChance; }
        }
        [SerializeField] private float _treeAppearanceChance = 0f;

        public float BuildingAppearanceChance {
            get { return _buildingAppearanceChance; }
        }
        [SerializeField] private float _buildingAppearanceChance = 0f;

        public float ResourceAppearanceChance {
            get { return _resourceAppearanceChance; }
        }
        [SerializeField] private float _resourceAppearanceChance = 0f;

        public float ImprovementAppearanceChance {
            get { return _improvementAppearanceChance; }
        }
        [SerializeField] private float _improvementAppearanceChance = 0f;

        public float RuinsAppearanceChance {
            get { return _ruinsAppearanceChance; }
        }
        [SerializeField] private float _ruinsAppearanceChance = 0f;

        public float EncampmentAppearanceChance {
            get { return _encampmentAppearanceChance; }
        }
        [SerializeField] private float _encampmentAppearanceChance = 0f;





        public int GuaranteedTreeModulo {
            get { return _guaranteedTreeModulo; }
        }
        [SerializeField] private int _guaranteedTreeModulo = 0;

        public int GuaranteedBuildingModulo {
            get { return _guaranteedBuildingModulo; }
        }
        [SerializeField] private int _guaranteedBuildingModulo = 0;

        public int GuaranteedResourceModulo {
            get { return _guaranteedResourceModulo; }
        }
        [SerializeField] private int _guaranteedResourceModulo = 0;

        public int GuaranteedImprovementModulo {
            get { return _guaranteedImprovementModulo; }
        }
        [SerializeField] private int _guaranteedImprovementModulo = 0;

        public int GuaranteedRuinsModulo {
            get { return _guaranteedRuinsModulo; }
        }
        [SerializeField] private int _guaranteedRuinsModulo = 0;

        public int GuaranteedEncampmentModulo {
            get { return _guaranteedEncampmentModulo; }
        }
        [SerializeField] private int _guaranteedEncampmentModulo = 0;

        #endregion

        #endregion

    }

}
