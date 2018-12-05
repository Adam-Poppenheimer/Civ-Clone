using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    [CreateAssetMenu(menuName = "Civ Clone/Promotion")]
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

        public bool PermitsLandTraversal {
            get { return _permitsLandTraversal; }
        }
        [SerializeField] private bool _permitsLandTraversal;

        public bool PermitsShallowWaterTraversal {
            get { return _permitsShallowWaterTraversal; }
        }
        [SerializeField] private bool _permitsShallowWaterTraversal;

        public bool PermitsDeepWaterTraversal {
            get { return _permitsDeepWaterTraversal; }
        }
        [SerializeField] private bool _permitsDeepWaterTraversal;

        public int BonusMovement {
            get { return _bonusMovement; }
        }
        [SerializeField] private int _bonusMovement;

        public int BonusVision {
            get { return _bonusVision; }
        }
        [SerializeField] private int _bonusVision;

        public IEnumerable<CellTerrain> TerrainsWithIgnoredCosts {
            get { return _terrainsWithIgnoredCosts; }
        }
        [SerializeField] private List<CellTerrain> _terrainsWithIgnoredCosts;

        public IEnumerable<CellShape> ShapesWithIgnoredCosts {
            get { return _shapesWithIgnoredCosts; }
        }
        [SerializeField] private List<CellShape> _shapesWithIgnoredCosts;

        public IEnumerable<CellVegetation> VegetationsWithIgnoredCosts {
            get { return _vegetationsWithIgnoredCosts; }
        }
        [SerializeField] private List<CellVegetation> _vegetationsWithIgnoredCosts;


        public IEnumerable<CellShape> ShapesConsumingFullMovement {
            get { return _shapesConsumingFullMovement; }
        }
        [SerializeField] private List<CellShape> _shapesConsumingFullMovement;

        public IEnumerable<CellVegetation> VegetationsConsumingFullMovement {
            get { return _vegetationsConsumingFullMovement; }
        }
        [SerializeField] private List<CellVegetation> _vegetationsConsumingFullMovement;


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

        public bool IgnoresLineOfSight {
            get { return _ignoresLineOfSight; }
        }
        [SerializeField] private bool _ignoresLineOfSight;

        public float GoldRaidingPercentage {
            get { return _goldRaidingPercentage; }
        }
        [SerializeField] private float _goldRaidingPercentage;

        public int BonusRange {
            get { return _bonusRange; }
        }
        [SerializeField] private int _bonusRange;

        public IEnumerable<ICombatModifier> ModifiersWhenAttacking {
            get {
                if(_attackModifiers == null) {
                    var castPermanent   = PermanentAttackModifiers.Cast<ICombatModifier>();
                    var castConditional = ConditionalAttackModifiers.Cast<ICombatModifier>();

                    _attackModifiers = castPermanent.Concat(castConditional).ToList();
                }
                return _attackModifiers;
            }
        }
        private List<ICombatModifier> _attackModifiers;
        [SerializeField] private List<PermanentCombatModifier>   PermanentAttackModifiers;
        [SerializeField] private List<ConditionalCombatModifier> ConditionalAttackModifiers;

        public IEnumerable<ICombatModifier> ModifiersWhenDefending {
            get {
                if(_defenseModifiers == null) {
                    var castPermanent   = PermanentDefenseModifiers.Cast<ICombatModifier>();
                    var castConditional = ConditionalDefenseModifiers.Cast<ICombatModifier>();

                    _defenseModifiers = castPermanent.Concat(castConditional).ToList();
                }
                return _defenseModifiers;
            }
        }
        private List<ICombatModifier> _defenseModifiers;
        [SerializeField] private List<PermanentCombatModifier>   PermanentDefenseModifiers;
        [SerializeField] private List<ConditionalCombatModifier> ConditionalDefenseModifiers;

        public IEnumerable<ICombatModifier> AuraModifiersWhenAttacking {
            get {
                if(_auraAttackModifiers == null) {
                    var castPermanent   = PermanentAuraAttackModifiers.Cast<ICombatModifier>();
                    var castConditional = ConditionalAuraAttackModifiers.Cast<ICombatModifier>();

                    _auraAttackModifiers = castPermanent.Concat(castConditional).ToList();
                }
                return _auraAttackModifiers;
            }
        }
        private List<ICombatModifier> _auraAttackModifiers;
        [SerializeField] private List<PermanentCombatModifier>   PermanentAuraAttackModifiers;
        [SerializeField] private List<ConditionalCombatModifier> ConditionalAuraAttackModifiers;

        public IEnumerable<ICombatModifier> AuraModifiersWhenDefending {
            get {
                if(_auraDefenseModifiers == null) {
                    var castPermanent   = PermanentAuraDefenseModifiers.Cast<ICombatModifier>();
                    var castConditional = ConditionalAuraDefenseModifiers.Cast<ICombatModifier>();

                    _auraDefenseModifiers = castPermanent.Concat(castConditional).ToList();
                }
                return _auraDefenseModifiers;
            }
        }
        private List<ICombatModifier> _auraDefenseModifiers;
        [SerializeField] private List<PermanentCombatModifier>   PermanentAuraDefenseModifiers;
        [SerializeField] private List<ConditionalCombatModifier> ConditionalAuraDefenseModifiers;





        public bool RequiresForeignTerritory {
            get { return _requiresForeignTerritory; }
        }
        [SerializeField] private bool _requiresForeignTerritory;

        public bool HealsEveryTurn {
            get { return _healsEveryTurn; }
        }
        [SerializeField] private bool _healsEveryTurn;

        public int BonusHealingToSelf {
            get { return _bonusHealingToSelf; }
        }
        [SerializeField] private int _bonusHealingToSelf;

        public int BonusHealingToAdjacent {
            get { return _bonusHealingToAdjacent; }
        }
        [SerializeField] private int _bonusHealingToAdjacent;

        public int AlternativeNavalBaseHealing {
            get { return _alternativeNavalBaseHealing; }
        }
        [SerializeField] private int _alternativeNavalBaseHealing;

        #endregion

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(DiContainer container) {
            var modifiersToInject = ConditionalAttackModifiers
                .Concat(ConditionalDefenseModifiers)
                .Concat(ConditionalAuraAttackModifiers)
                .Concat(ConditionalAuraDefenseModifiers);

            foreach(var modifier in modifiersToInject) {
                container.Inject(modifier);
            }
        }


        #endregion

    }

}
