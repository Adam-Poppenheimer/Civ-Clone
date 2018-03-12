using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units.Abilities;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Units {

    [CreateAssetMenu(menuName = "Civ Clone/Unit")]
    public class UnitTemplate : ScriptableObject, IUnitTemplate {

        #region instance fields and properties

        #region from IUnitTemplate

        public string Name {
            get { return _name; }
        }
        [SerializeField] private string _name;

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

        public IEnumerable<ISpecialtyResourceDefinition> RequiredResources {
            get { return _requiredResources.Cast<ISpecialtyResourceDefinition>(); }
        }
        [SerializeField] private List<SpecialtyResourceDefinition> _requiredResources;

        public bool BenefitsFromDefensiveTerrain {
            get { return _benefitsFromDefensiveTerrain; }
        }
        [SerializeField] private bool _benefitsFromDefensiveTerrain;

        public bool IgnoresTerrainCosts {
            get { return _ignoresTerrainCosts; }
        }
        [SerializeField] private bool _ignoresTerrainCosts;

        #endregion

        #endregion
        
    }

}
