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

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        public YieldSummary BonusYieldNormal {
            get { return _bonusYield; }
        }
        [SerializeField] private YieldSummary _bonusYield;

        public IEnumerable<CellVegetation> RestrictedToVegetations {
            get { return _restrictedToVegetations; }
        }
        [SerializeField] private List<CellVegetation> _restrictedToVegetations;

        public IEnumerable<CellTerrain> RestrictedToTerrains {
            get { return _restrictedToTerrains; }
        }
        [SerializeField] private List<CellTerrain> _restrictedToTerrains;

        public IEnumerable<CellShape> RestrictedToShapes {
            get { return _restrictedToShapes; }
        }
        [SerializeField] private List<CellShape> _restrictedToShapes;

        public bool ClearsVegetationWhenBuilt {
            get { return _clearsVegetationWhenBuilt; }
        }
        [SerializeField] private bool _clearsVegetationWhenBuilt;

        public float DefensiveBonus {
            get { return _defensiveBonus; }
        }
        [SerializeField] private float _defensiveBonus;

        public bool RequiresResourceToExtract {
            get { return _requiresResourceToExtract; }
        }
        [SerializeField] private bool _requiresResourceToExtract;

        public Transform AppearancePrefab {
            get { return _appearancePrefab; }
        }
        [SerializeField] private Transform _appearancePrefab;

        public int TurnsToConstruct {
            get { return _turnsToConstruct; }
        }
        [SerializeField] private int _turnsToConstruct;

        public bool FreshWaterAlwaysEnables {
            get { return _freshWaterAlwaysEnables; }
        }
        [SerializeField] private bool _freshWaterAlwaysEnables;

        public float AdjacentEnemyDamagePercentage {
            get { return _adjacentEnemyDamagePercentage; }
        }
        [SerializeField] private float _adjacentEnemyDamagePercentage;

        #endregion

        #endregion
        
    }

}
