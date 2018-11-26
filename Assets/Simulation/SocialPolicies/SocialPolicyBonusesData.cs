using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.SocialPolicies {

    [Serializable]
    public struct SocialPolicyBonusesData : ISocialPolicyBonusesData {

        #region instance fields and properties

        public YieldSummary CapitalYield {
            get { return _capitalYield; }
        }
        [SerializeField] private YieldSummary _capitalYield;

        public YieldSummary CityYield {
            get { return _cityYield; }
        }
        [SerializeField] private YieldSummary _cityYield;

        public float CapitalBorderExpansionModifier  {
            get { return _capitalBorderExpansionModifier; }
        }
        [SerializeField] private float _capitalBorderExpansionModifier;

        public float CityBorderExpansionModifier  {
            get { return _cityBorderExpansionModifier; }
        }
        [SerializeField] private float _cityBorderExpansionModifier;

        public float CapitalGrowthModifier {
            get { return _capitalGrowthModifier; }
        }
        [SerializeField] private float _capitalGrowthModifier;

        public float CityGrowthModifier {
            get { return _cityGrowthModifier; }
        }
        [SerializeField] private float _cityGrowthModifier;

        public float CityWonderProductionModifier {
            get { return _cityWonderProductionModifier; }
        }
        [SerializeField] private float _cityWonderProductionModifier;

        public float CapitalHappinessPerPopulation {
            get { return _capitalHappinessPerPopulation; }
        }
        [SerializeField] private float _capitalHappinessPerPopulation;

        public float CityHappinessPerPopulation {
            get { return _cityHappinessPerPopulation; }
        }
        [SerializeField] private float _cityHappinessPerPopulation;

        public float CapitalUnhappinessPerPopulation {
            get { return _capitalUnhappinessPerPopulation; }
        }
        [SerializeField] private float _capitalUnhappinessPerPopulation;

        public float CityUnhappinessPerPopulation {
            get { return _cityUnhappinessPerPopulation; }
        }
        [SerializeField] private float _cityUnhappinessPerPopulation;

        public bool SuppressesGarrisionedUnitMaintenance {
            get { return _suppressesGarrisionedUnitMaintenance; }
        }
        [SerializeField] private bool _suppressesGarrisionedUnitMaintenance;

        public float GarrisionedCityBonusStrength {
            get { return _garrisionedCityBonusStrength; }
        }
        [SerializeField] private float _garrisionedCityBonusStrength;

        #endregion

    }

}
