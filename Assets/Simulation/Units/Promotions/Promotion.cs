using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public class Promotion : ScriptableObject, IPromotion {

        #region instance fields and properties

        #region from IPromotion

        public string Name {
            get { return name; }
        }

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        #endregion

        public bool RestrictedByTerrains {
            get { return _restrictedByTerrains; }
        }
        [SerializeField] private bool _restrictedByTerrains;

        public IEnumerable<TerrainType> ValidTerrains {
            get { return _validTerrains; }
        }
        [SerializeField] private List<TerrainType> _validTerrains;

        public bool RestrictedByShapes {
            get { return _restrictedByShapes; }
        }
        [SerializeField] private bool _restrictedByShapes;

        public IEnumerable<TerrainShape> ValidShapes {
            get { return _validShapes; }
        }
        [SerializeField] private List<TerrainShape> _validShapes;

        public bool RestrictedByFeatures {
            get { return _restrictedByFeatures; }
        }
        [SerializeField] private bool _restrictedByFeatures;

        public IEnumerable<TerrainFeature> ValidFeatures {
            get { return _validFeatures; }
        }
        [SerializeField] private List<TerrainFeature> _validFeatures;

        public bool RestrictedByOpponentTypes {
            get { return _restrictedByOpponentTypes; }
        }
        [SerializeField] private bool _restrictedByOpponentTypes;

        public IEnumerable<UnitType> ValidOpponentTypes {
            get { return _validOpponentTypes; }
        }
        [SerializeField] private List<UnitType> _validOpponentTypes;

        public bool RequiresFlatTerrain {
            get { return _requiresFlatTerrain; }
        }
        [SerializeField] private bool _requiresFlatTerrain;

        public bool RequiresRoughTerrain {
            get { return _requiresRoughTerrain; }
        }
        [SerializeField] private bool _requiresRoughTerrain;

        public bool RestrictedByCombatType {
            get { return _restrictedByCombatType; }
        }
        [SerializeField] private bool _restrictedByCombatType;

        public CombatType ValidCombatType {
            get { return _validCombatType; }
        }
        [SerializeField] private CombatType _validCombatType;

        public bool AppliesWhileAttacking {
            get { return _appliesWhileAttacking; }
        }
        [SerializeField] private bool _appliesWhileAttacking;

        public bool AppliesWhileDefending {
            get { return _appliesWhileDefending; }
        }
        [SerializeField] private bool _appliesWhileDefending;

        public float CombatModifier {
            get { return _combatModifier; }
        }
        [SerializeField] private float _combatModifier;
        
        public bool CanMoveAfterAttacking {
            get { return _canMoveAfterAttacking; }
        }
        [SerializeField] private bool _canMoveAfterAttacking;

        public bool CanAttackAfterAttacking {
            get { return _canAttackAfterAttacking; }
        }
        [SerializeField] private bool _canAttackAfterAttacking;

        public bool IgnoresAmphibiousPenalty {
            get { return _ignoresAmphibiousPenalty; }
        }
        [SerializeField] private bool _ignoresAmphibiousPenalty;

        public bool IgnoresDefensiveTerrainBonuses {
            get { return _ignoresDefensiveTerrainBonuses; }
        }
        [SerializeField] private bool _ignoresDefensiveTerrainBonuses;

        public float GoldRaidingPercentage {
            get { return _goldRaidingPercentage; }
        }
        [SerializeField] private float _goldRaidingPercentage;

        public bool IgnoresLOSWhenAttacking {
            get { return _ignoresLOSWhenAttacking; }
        }
        [SerializeField] private bool _ignoresLOSWhenAttacking;

        public bool IgnoresTerrainCosts {
            get { return _ignoresTerrainCosts; }
        }
        [SerializeField] private bool _ignoresTerrainCosts;

        #endregion

    }

}
