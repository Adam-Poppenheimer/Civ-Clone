using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The standard implementation of ICivilizationConfig
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/Civilizations/Config")]
    public class CivilizationConfig : ScriptableObject, ICivilizationConfig {

        #region instance fields and properties

        #region from ICivilizationConfig

        public int BaseHappiness {
            get { return _baseHappiness; }
        }
        [SerializeField] private int _baseHappiness;

        public int HappinessPerLuxury {
            get { return _happinessPerLuxury; }
        }
        [SerializeField] private int _happinessPerLuxury;

        public float YieldLossPerUnhappiness {
            get { return _yieldLossPerUnhappiness; }
        }
        [SerializeField] private float _yieldLossPerUnhappiness;

        public float ModifierLossPerUnhappiness {
            get { return _modifierLossPerUnhappiness; }
        }
        [SerializeField] private float _modifierLossPerUnhappiness;

        public int BasePolicyCost {
            get { return _basePolicyCost; }
        }
        [SerializeField] private int _basePolicyCost;

        public float PolicyCostPerPolicyCoefficient {
            get { return _policyCostPerPolicyCoefficient; }
        }
        [SerializeField] private float _policyCostPerPolicyCoefficient;

        public float PolicyCostPerPolicyExponent {
            get { return _policyCostPerPolicyExponent; }
        }
        [SerializeField] private float _policyCostPerPolicyExponent;

        public float PolicyCostPerCityCoefficient {
            get { return _policyCostPerCityCoefficient; }
        }
        [SerializeField] private float _policyCostPerCityCoefficient;

        public CivilizationDefeatMode DefeatMode {
            get { return _defeatMode; }
        }
        [SerializeField] private CivilizationDefeatMode _defeatMode;

        public int MaintenanceFreeUnits {
            get { return _maintenanceFreeUnits; }
        }
        [SerializeField] private int _maintenanceFreeUnits;

        public float CivilianGreatPersonStartingCost {
            get { return _civilizationGreatPersonStartingCost; }
        }
        [SerializeField] private float _civilizationGreatPersonStartingCost;

        public float MilitaryGreatPersonStartingCost {
            get { return _militaryGreatPersonStartingCost; }
        }
        [SerializeField] private float _militaryGreatPersonStartingCost;

        public float GreatPersonPredecessorCostMultiplier {
            get { return _greatPersonPredecessorCostMultiplier; }
        }
        [SerializeField] private float _greatPersonPredecessorCostMultiplier;

        public float ExperienceToGreatPersonPointRatio {
            get { return _experienceToGreatPersonPointRatio; }
        }
        [SerializeField] private float _experienceToGreatPersonPointRatio = 1f;

        public float GoldenAgeBaseCost {
            get { return _goldenAgeBaseCost; }
        }
        [SerializeField] private float _goldenAgeBaseCost;

        public float GoldenAgeCostPerPreviousAge {
            get { return _goldenAgeCostPerPreviousAge; }
        }
        [SerializeField] private float _goldenAgeCostPerPreviousAge;

        public float GoldenAgePerCityMultiplier {
            get { return _goldenAgePerCityMultiplier; }
        }
        [SerializeField] private float _goldenAgePerCityMultiplier;

        public float GoldenAgeBaseLength {
            get { return _goldenAgeBaseLength; }
        }
        [SerializeField] private float _goldenAgeBaseLength;

        public YieldSummary GoldenAgeProductionModifiers {
            get { return _goldenAgeProductionModifiers; }
        }
        [SerializeField] private YieldSummary _goldenAgeProductionModifiers;

        public int GoldenAgeBonusGoldOnCells {
            get { return _goldenAgeBonusGoldOnCells; }
        }
        [SerializeField] private int _goldenAgeBonusGoldOnCells;

        public ICivilizationTemplate DefaultTemplate {
            get { return _defaultTemplate; }
        }
        [SerializeField] private CivilizationTemplate _defaultTemplate;

        public ICivilizationTemplate BarbarianTemplate {
            get { return _barbarianTemplate; }
        }
        [SerializeField] private CivilizationTemplate _barbarianTemplate;

        #endregion

        #endregion

    }

}
