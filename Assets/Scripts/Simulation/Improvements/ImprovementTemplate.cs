using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    [CreateAssetMenu(menuName = "Civ Clone/Improvements/Improvement")]
    public class ImprovementTemplate : ScriptableObject, IImprovementTemplate {

        #region instance fields and properties

        #region from IImprovementTemplate

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon = null;

        public YieldSummary BonusYieldNormal {
            get { return _bonusYield; }
        }
        [SerializeField] private YieldSummary _bonusYield = YieldSummary.Empty;

        public IEnumerable<CellVegetation> RestrictedToVegetations {
            get { return _restrictedToVegetations; }
        }
        [SerializeField] private List<CellVegetation> _restrictedToVegetations = null;

        public IEnumerable<CellTerrain> RestrictedToTerrains {
            get { return _restrictedToTerrains; }
        }
        [SerializeField] private List<CellTerrain> _restrictedToTerrains = null;

        public IEnumerable<CellShape> RestrictedToShapes {
            get { return _restrictedToShapes; }
        }
        [SerializeField] private List<CellShape> _restrictedToShapes = null;

        public bool ClearsVegetationWhenBuilt {
            get { return _clearsVegetationWhenBuilt; }
        }
        [SerializeField] private bool _clearsVegetationWhenBuilt = false;

        public float DefensiveBonus {
            get { return _defensiveBonus; }
        }
        [SerializeField] private float _defensiveBonus = 0f;

        public bool RequiresResourceToExtract {
            get { return _requiresResourceToExtract; }
        }
        [SerializeField] private bool _requiresResourceToExtract = false;

        public Transform AppearancePrefab {
            get { return _appearancePrefab; }
        }
        [SerializeField] private Transform _appearancePrefab = null;

        public int TurnsToConstruct {
            get { return _turnsToConstruct; }
        }
        [SerializeField] private int _turnsToConstruct = 0;

        public bool FreshWaterAlwaysEnables {
            get { return _freshWaterAlwaysEnables; }
        }
        [SerializeField] private bool _freshWaterAlwaysEnables = false;

        public float AdjacentEnemyDamagePercentage {
            get { return _adjacentEnemyDamagePercentage; }
        }
        [SerializeField] private float _adjacentEnemyDamagePercentage = 0f;

        public bool OverridesTerrain {
            get { return _overridesTerrain; }
        }
        [SerializeField] private bool _overridesTerrain = false;

        public int OverridingTerrainIndex {
            get { return _overridingTerrainIndex; }
        }
        [SerializeField] private int _overridingTerrainIndex = 0;

        public bool ProducesFarmland {
            get { return _producesFarmland; }
        }
        [SerializeField] private bool _producesFarmland = false;

        #endregion

        #endregion
        
    }

}
