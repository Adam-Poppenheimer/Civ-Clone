using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Units;

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

        public bool SuppressesGarrisonedUnitMaintenance {
            get { return _suppressesGarrisonedUnitMaintenance; }
        }
        [SerializeField] private bool _suppressesGarrisonedUnitMaintenance;

        public float GarrisonedCityBonusStrength {
            get { return _garrisionedCityBonusStrength; }
        }
        [SerializeField] private float _garrisionedCityBonusStrength;

        public IEnumerable<IBuildingTemplate> FreeBuildingTemplates {
            get { return _freeBuildingTemplates.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _freeBuildingTemplates;

        public int FreeBuildingCount {
            get { return _freeBuildingCount; }
        }
        [SerializeField] private int _freeBuildingCount;

        public IProductionModifier ProductionModifier {
            get { return _productionModifier; }
        }
        [SerializeField] private ProductionModifier _productionModifier;

        public IEnumerable<IUnitTemplate> FreeUnits {
            get { return _freeUnits.Cast<IUnitTemplate>(); }
        }
        [SerializeField] private List<UnitTemplate> _freeUnits;

        public float PolicyCostFromCityCountModifier {
            get { return _policyCostFromCityCountModifier; }
        }
        [SerializeField] private float _policyCostFromCityCountModifier;

        #endregion

    }

}
