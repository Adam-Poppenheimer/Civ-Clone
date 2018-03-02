using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    [CreateAssetMenu(menuName = "Civ Clone/Improvement")]
    public class ImprovementTemplate : ScriptableObject, IImprovementTemplate {

        #region instance fields and properties

        #region from IImprovementTemplate

        public ResourceSummary BonusYieldNormal {
            get { return _bonusYield; }
        }
        [SerializeField] private ResourceSummary _bonusYield;

        public IEnumerable<TerrainFeature> RestrictedToFeatures {
            get { return _restrictedToFeatures; }
        }
        [SerializeField] private List<TerrainFeature> _restrictedToFeatures;

        public IEnumerable<TerrainType> RestrictedToTerrains {
            get { return _restrictedToTerrains; }
        }
        [SerializeField] private List<TerrainType> _restrictedToTerrains;

        public IEnumerable<TerrainShape> RestrictedToShapes {
            get { return _restrictedToShapes; }
        }
        [SerializeField] private List<TerrainShape> _restrictedToShapes;

        public bool ClearsForestsWhenBuilt {
            get { return _clearsForestsWhenBuilt; }
        }
        [SerializeField] private bool _clearsForestsWhenBuilt;

        public float DefensiveBonus {
            get { return _defensiveBonus; }
        }
        [SerializeField] private float _defensiveBonus;

        public Transform AppearancePrefab {
            get { return _appearancePrefab; }
        }
        [SerializeField] private Transform _appearancePrefab;

        #endregion

        #endregion
        
    }

}
