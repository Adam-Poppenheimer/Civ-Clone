using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Units {

    [CreateAssetMenu(menuName = "Civ Clone/Units/Unit")]
    public class UnitTemplate : ScriptableObject, IUnitTemplate {

        #region instance fields and properties

        #region from IUnitTemplate

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description = null;

        public Sprite Image {
            get { return _image; }
        }
        [SerializeField] private Sprite _image = null;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon = null;

        public GameObject DisplayPrefab {
            get { return _displayPrefab; }
        }
        [SerializeField] private GameObject _displayPrefab = null;

        public int MaxMovement {
            get { return _maxMovement; }
        }
        [SerializeField] private int _maxMovement = 0;

        public int ProductionCost {
            get { return _productionCost; }
        }
        [SerializeField] private int _productionCost = 0;

        public UnitType Type {
            get { return _type; }
        }
        [SerializeField] private UnitType _type = UnitType.Archery;

        public IEnumerable<IAbilityDefinition> Abilities {
            get { return _abilities.Cast<IAbilityDefinition>(); }
        }
        [SerializeField] private List<AbilityDefinition> _abilities = null;

        public int AttackRange {
            get { return _attackRange; }
        }
        [SerializeField] private int _attackRange = 0;

        public int CombatStrength {
            get { return _combatStrength; }
        }
        [SerializeField] private int _combatStrength = 0;

        public int RangedAttackStrength {
            get { return _rangedAttackStrength; }
        }
        [SerializeField] private int _rangedAttackStrength = 0;

        public int MaxHitpoints {
            get { return _maxHitpoints; }
        }
        [SerializeField] private int _maxHitpoints = 0;

        public IEnumerable<IResourceDefinition> RequiredResources {
            get { return _requiredResources.Cast<IResourceDefinition>(); }
        }
        [SerializeField] private List<ResourceDefinition> _requiredResources = null;

        public bool IsImmobile {
            get { return _isImmobile; }
        }
        [SerializeField] private bool _isImmobile = false;

        public bool MustSetUpToBombard {
            get { return _mustSetUpToBombard; }
        }
        [SerializeField] private bool _mustSetUpToBombard = false;

        public int VisionRange {
            get { return _visionRange; }
        }
        [SerializeField] private int _visionRange = 0;

        public bool IgnoresLineOfSight {
            get { return _ignoresLineOfSight; }
        }
        [SerializeField] private bool _ignoresLineOfSight = false;

        public IEnumerable<IPromotion> StartingPromotions {
            get { return _startingPromotions.Cast<IPromotion>(); }
        }
        [SerializeField] private List<Promotion> _startingPromotions = null;

        public IPromotionTreeTemplate PromotionTreeData {
            get { return _promotionTreeData; }
        }
        [SerializeField] private PromotionTreeTemplate _promotionTreeData = null;

        public IUnitMovementSummary MovementSummary { get; private set; }

        #endregion

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IPromotionParser promotionParser) {
            var movementSummary = new UnitMovementSummary();

            promotionParser.SetMovementSummary(movementSummary, StartingPromotions);

            MovementSummary = movementSummary;
        }

        #endregion

    }

}
