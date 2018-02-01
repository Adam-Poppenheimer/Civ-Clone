using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// The standard implementation of ICityConfig.
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/City Config")]
    public class CityConfig : ScriptableObject, ICityConfig {

        #region instance fields and properties

        #region from ICityConfig

        /// <inheritdoc/>
        public int BaseGrowthStockpile {
            get { return _baseGrowthStockpile; }
        }
        [SerializeField] private int _baseGrowthStockpile;

        /// <inheritdoc/>
        public int FoodConsumptionPerPerson {
            get { return _foodConsumptionPerPerson; }
        }
        [SerializeField] private int _foodConsumptionPerPerson;

        /// <inheritdoc/>
        public int GrowthPreviousPopulationCoefficient {
            get { return _growthPreviousPopulationCoefficient; }
        }
        [SerializeField] private int _growthPreviousPopulationCoefficient;

        /// <inheritdoc/>
        public float GrowthPreviousPopulationExponent {
            get { return _growthPreviousPopulationExponent; }
        }
        [SerializeField] private float _growthPreviousPopulationExponent;

        /// <inheritdoc/>
        public float HurryCostPerProduction {
            get { return _hurryCostPerProduction; }
        }
        [SerializeField] private float _hurryCostPerProduction;

        /// <inheritdoc/>
        public int MaxBorderRange {
            get { return _maxBorderRange; }
        }
        [SerializeField] private int _maxBorderRange;

        /// <inheritdoc/>
        public int PreviousTileCountCoefficient {
            get { return _previousTileCountCoefficient; }
        }
        [SerializeField] private int _previousTileCountCoefficient;

        /// <inheritdoc/>
        public float PreviousTileCountExponent {
            get { return _previousTileCountExponent; }
        }
        [SerializeField] private float _previousTileCountExponent;

        /// <inheritdoc/>
        public int TileCostBase {
            get { return _tileCostBase; }
        }
        [SerializeField] private int _tileCostBase;

        /// <inheritdoc/>
        public ResourceSummary UnemployedYield {
            get { return _unemployedYield; }
        }
        [SerializeField] private ResourceSummary _unemployedYield;

        /// <inheritdoc/>
        public int MinimumSeparation {
            get { return _minimumSeparation; }
        }
        [SerializeField] private int _minimumSeparation;

        public int CityAttackRange {
            get { return _cityAttackRange; }
        }
        [SerializeField] private int _cityAttackRange;

        public int BaseCombatStrength {
            get { return _baseCombatStrength; }
        }
        [SerializeField] private int _baseCombatStrength;

        public float CombatStrengthPerPopulation {
            get { return _combatStrengthPerPopulation; }
        }
        [SerializeField] private float _combatStrengthPerPopulation;

        public int BaseMaxHitPoints {
            get { return _baseMaxHitPoints; }
        }
        [SerializeField] private int _baseMaxHitPoints;

        public float MaxHitPointsPerPopulation {
            get { return _maxHitPointsPerPopulation; }
        }
        [SerializeField] private float _maxHitPointsPerPopulation;

        public int BaseRangedAttackStrength {
            get { return _baseRangedAttackStrength; }
        }
        [SerializeField] private int _baseRangedAttackStrength;

        public float RangedAttackStrengthPerPopulation {
            get { return _rangedAttackStrengthPerPopulation; }
        }
        [SerializeField] private float _rangedAttackStrengthPerPopulation;

        public int HitPointRegenPerRound {
            get { return _hitPointRegenPerRound; }
        }
        [SerializeField] private int _hitPointRegenPerRound;

        public int VisionRange {
            get { return _visionRange; }
        }
        [SerializeField] private int _visionRange;

        public int BaseHealth {
            get { return _baseHealth; }
        }
        [SerializeField] private int _baseHealth;

        #endregion

        #endregion
        
    }

}
