using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    [CreateAssetMenu(menuName = "Civ Clone/Improvement Template")]
    public class ImprovementTemplate : ScriptableObject, IImprovementTemplate {

        #region instance fields and properties

        #region from IImprovementTemplate

        public ResourceSummary BonusYield {
            get { return _bonusYield; }
        }
        [SerializeField] private ResourceSummary _bonusYield;

        public IEnumerable<TerrainFeatureType> ValidFeatures {
            get { return _validFeatures; }
        }
        [SerializeField] private List<TerrainFeatureType> _validFeatures;

        public IEnumerable<TerrainShape> ValidShapes {
            get { return _validShapes; }
        }
        [SerializeField] private List<TerrainShape> _validShapes;

        public IEnumerable<TerrainType> ValidTerrains {
            get { return _validTerrains; }
        }
        [SerializeField] private List<TerrainType> _validTerrains;

        public float WorkToComplete {
            get { return _workToComplete; }
        }
        [SerializeField] private float _workToComplete;

        #endregion

        #endregion
        
    }

}
