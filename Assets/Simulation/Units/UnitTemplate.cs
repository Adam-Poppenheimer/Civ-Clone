using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Units {

    [CreateAssetMenu(menuName = "Civ Clone/Unit")]
    public class UnitTemplate : ScriptableObject, IUnitTemplate {

        #region instance fields and properties

        #region from IUnitTemplate

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description;

        public Sprite Image {
            get { return _image; }
        }
        [SerializeField] private Sprite _image;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        public GameObject Prefab {
            get { return _prefab; }
        }
        [SerializeField] private GameObject _prefab;

        public int MaxMovement {
            get { return _maxMovement; }
        }
        [SerializeField] private int _maxMovement;

        public int ProductionCost {
            get { return _productionCost; }
        }
        [SerializeField] private int _productionCost;

        public UnitType Type {
            get { return _type; }
        }
        [SerializeField] private UnitType _type;

        public bool IsAquatic {
            get { return _isAquatic; }
        }
        [SerializeField] private bool _isAquatic;

        public IEnumerable<IAbilityDefinition> Abilities {
            get { return _abilities.Cast<IAbilityDefinition>(); }
        }
        [SerializeField] private List<AbilityDefinition> _abilities;

        public int AttackRange {
            get { return _attackRange; }
        }
        [SerializeField] private int _attackRange;

        public int CombatStrength {
            get { return _combatStrength; }
        }
        [SerializeField] private int _combatStrength;

        public int RangedAttackStrength {
            get { return _rangedAttackStrength; }
        }
        [SerializeField] private int _rangedAttackStrength;

        public int MaxHitpoints {
            get { return _maxHitpoints; }
        }
        [SerializeField] private int _maxHitpoints;

        public IEnumerable<ISpecialtyResourceDefinition> RequiredResources {
            get { return _requiredResources.Cast<ISpecialtyResourceDefinition>(); }
        }
        [SerializeField] private List<SpecialtyResourceDefinition> _requiredResources;

        public bool IsImmobile {
            get { return _isImmobile; }
        }
        [SerializeField] private bool _isImmobile;

        public bool MustSetUpToBombard {
            get { return _mustSetUpToBombard; }
        }
        [SerializeField] private bool _mustSetUpToBombard;

        public int VisionRange {
            get { return _visionRange; }
        }
        [SerializeField] private int _visionRange;

        public bool IgnoresLineOfSight {
            get { return _ignoresLineOfSight; }
        }
        [SerializeField] private bool _ignoresLineOfSight;

        public IEnumerable<IPromotion> StartingPromotions {
            get { return _startingPromotions.Cast<IPromotion>(); }
        }
        [SerializeField] private List<Promotion> _startingPromotions;

        #endregion

        #endregion
        
    }

}
