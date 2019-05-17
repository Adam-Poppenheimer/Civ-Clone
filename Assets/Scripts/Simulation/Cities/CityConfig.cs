using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// The standard implementation of ICityConfig.
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/Cities/Config")]
    public class CityConfig : ScriptableObject, ICityConfig {

        #region instance fields and properties

        #region from ICityConfig

        /// <inheritdoc/>
        public int BaseGrowthStockpile {
            get { return _baseGrowthStockpile; }
        }
        [SerializeField] private int _baseGrowthStockpile = 0;

        /// <inheritdoc/>
        public int FoodConsumptionPerPerson {
            get { return _foodConsumptionPerPerson; }
        }
        [SerializeField] private int _foodConsumptionPerPerson = 0;

        /// <inheritdoc/>
        public int GrowthPreviousPopulationCoefficient {
            get { return _growthPreviousPopulationCoefficient; }
        }
        [SerializeField] private int _growthPreviousPopulationCoefficient = 0;

        /// <inheritdoc/>
        public float GrowthPreviousPopulationExponent {
            get { return _growthPreviousPopulationExponent; }
        }
        [SerializeField] private float _growthPreviousPopulationExponent = 0f;

        /// <inheritdoc/>
        public float HurryCostPerProduction {
            get { return _hurryCostPerProduction; }
        }
        [SerializeField] private float _hurryCostPerProduction = 0f;

        public float HurryAbilityBaseProduction {
            get { return _hurryAbilityBaseProduction; }
        }
        [SerializeField] private float _hurryAbilityBaseProduction = 0f;

        public float HurryAbilityPerPopProduction {
            get { return _hurryAbilityPerPopProduction; }
        }
        [SerializeField] private float _hurryAbilityPerPopProduction = 0f;

        /// <inheritdoc/>
        public int MaxBorderRange {
            get { return _maxBorderRange; }
        }
        [SerializeField] private int _maxBorderRange = 0;

        /// <inheritdoc/>
        public int PreviousCellCountCoefficient {
            get { return _previousTileCountCoefficient; }
        }
        [SerializeField] private int _previousTileCountCoefficient = 0;

        /// <inheritdoc/>
        public float PreviousCellCountExponent {
            get { return _previousTileCountExponent; }
        }
        [SerializeField] private float _previousTileCountExponent = 0f;

        /// <inheritdoc/>
        public int CellCostBase {
            get { return _tileCostBase; }
        }
        [SerializeField] private int _tileCostBase = 0;

        /// <inheritdoc/>
        public YieldSummary UnemployedYield {
            get { return _unemployedYield; }
        }
        [SerializeField] private YieldSummary _unemployedYield = YieldSummary.Empty;

        /// <inheritdoc/>
        public int MinimumSeparation {
            get { return _minimumSeparation; }
        }
        [SerializeField] private int _minimumSeparation = 0;

        public int CityAttackRange {
            get { return _cityAttackRange; }
        }
        [SerializeField] private int _cityAttackRange = 0;

        public int BaseCombatStrength {
            get { return _baseCombatStrength; }
        }
        [SerializeField] private int _baseCombatStrength = 0;

        public float CombatStrengthPerPopulation {
            get { return _combatStrengthPerPopulation; }
        }
        [SerializeField] private float _combatStrengthPerPopulation = 0f;

        public int BaseMaxHitPoints {
            get { return _baseMaxHitPoints; }
        }
        [SerializeField] private int _baseMaxHitPoints = 0;

        public float MaxHitPointsPerPopulation {
            get { return _maxHitPointsPerPopulation; }
        }
        [SerializeField] private float _maxHitPointsPerPopulation = 0f;

        public int BaseRangedAttackStrength {
            get { return _baseRangedAttackStrength; }
        }
        [SerializeField] private int _baseRangedAttackStrength = 0;

        public float RangedAttackStrengthPerPopulation {
            get { return _rangedAttackStrengthPerPopulation; }
        }
        [SerializeField] private float _rangedAttackStrengthPerPopulation = 0f;

        public int VisionRange {
            get { return _visionRange; }
        }
        [SerializeField] private int _visionRange = 0;

        public int UnhappinessPerCity {
            get { return _unhappinessPerCity; }
        }
        [SerializeField] private int _unhappinessPerCity = 0;

        public float UnhappinessPerPopulation {
            get { return _unhappinessPerPopulation; }
        }
        [SerializeField] private float _unhappinessPerPopulation = 0f;

        public YieldSummary CityCenterBaseYield {
            get { return _cityCenterBaseYield; }
        }
        [SerializeField] private YieldSummary _cityCenterBaseYield = YieldSummary.Empty;

        public Sprite CombatantImage {
            get { return _combatantImage; }
        }
        [SerializeField] private Sprite _combatantImage = null;

        public Sprite CombatantIcon {
            get { return _combatantIcon; }
        }
        [SerializeField] private Sprite _combatantIcon = null;

        public GameObject CombatantDisplayPrefab {
            get { return _combatantDisplayPrefab; }
        }
        [SerializeField] private GameObject _combatantDisplayPrefab = null;

        public IEnumerable<IBuildingTemplate> CapitalTemplates {
            get { return _capitalTemplates.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _capitalTemplates = null;

        #endregion

        #endregion
        
    }

}
