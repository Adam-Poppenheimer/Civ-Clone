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

        public ResourceSummary BonusYield {
            get { return _bonusYield; }
        }
        [SerializeField] private ResourceSummary _bonusYield;

        public IEnumerable<TerrainFeature> ValidFeatures {
            get { return _validFeatures; }
        }
        [SerializeField] private List<TerrainFeature> _validFeatures;

        public IEnumerable<TerrainType> ValidTerrains {
            get { return _validTerrains; }
        }
        [SerializeField] private List<TerrainType> _validTerrains;

        public bool RequiresAdjacentUpwardCliff {
            get { return _requiresAdjacentUpwardCliff; }
        }
        [SerializeField] private bool _requiresAdjacentUpwardCliff;

        public bool ClearsForestsWhenBuilt {
            get { return _clearsForestsWhenBuilt; }
        }
        [SerializeField] private bool _clearsForestsWhenBuilt;

        public Transform AppearancePrefab {
            get { return _appearancePrefab; }
        }
        [SerializeField] private Transform _appearancePrefab;

        #endregion

        #endregion
        
    }

}
