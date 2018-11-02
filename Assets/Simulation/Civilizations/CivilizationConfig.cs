using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The standard implementation of ICivilizationConfig
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/Civilization Config")]
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

        #endregion

        #endregion

    }

}
