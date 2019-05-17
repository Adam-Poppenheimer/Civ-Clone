using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.SocialPolicies {

    [Serializable]
    public class SocialPolicyBonusesData : ISocialPolicyBonusesData {

        #region instance fields and properties

        public YieldSummary CapitalYield {
            get { return _capitalYield; }
        }
        [SerializeField] private YieldSummary _capitalYield = YieldSummary.Empty;

        public YieldSummary CityYield {
            get { return _cityYield; }
        }
        [SerializeField] private YieldSummary _cityYield = YieldSummary.Empty;

        public float CapitalBorderExpansionModifier  {
            get { return _capitalBorderExpansionModifier; }
        }
        [SerializeField] private float _capitalBorderExpansionModifier = 0f;

        public float CityBorderExpansionModifier  {
            get { return _cityBorderExpansionModifier; }
        }
        [SerializeField] private float _cityBorderExpansionModifier = 0f;

        public float CapitalGrowthModifier {
            get { return _capitalGrowthModifier; }
        }
        [SerializeField] private float _capitalGrowthModifier = 0f;

        public float CityGrowthModifier {
            get { return _cityGrowthModifier; }
        }
        [SerializeField] private float _cityGrowthModifier = 0f;

        public float CapitalHappinessPerPopulation {
            get { return _capitalHappinessPerPopulation; }
        }
        [SerializeField] private float _capitalHappinessPerPopulation = 0f;

        public float CityHappinessPerPopulation {
            get { return _cityHappinessPerPopulation; }
        }
        [SerializeField] private float _cityHappinessPerPopulation = 0f;

        public float CapitalUnhappinessPerPopulation {
            get { return _capitalUnhappinessPerPopulation; }
        }
        [SerializeField] private float _capitalUnhappinessPerPopulation = 0f;

        public float CityUnhappinessPerPopulation {
            get { return _cityUnhappinessPerPopulation; }
        }
        [SerializeField] private float _cityUnhappinessPerPopulation = 0f;

        public int ConnectedToCapitalHappiness {
            get { return _connectedToCapitalHappiness; }
        }
        [SerializeField] private int _connectedToCapitalHappiness = 0;

        public bool SuppressesGarrisonedUnitMaintenance {
            get { return _suppressesGarrisonedUnitMaintenance; }
        }
        [SerializeField] private bool _suppressesGarrisonedUnitMaintenance = false;

        public float GarrisonedCityBonusStrength {
            get { return _garrisionedCityBonusStrength; }
        }
        [SerializeField] private float _garrisionedCityBonusStrength = 0f;

        public IEnumerable<IBuildingTemplate> FreeBuildingTemplates {
            get { return _freeBuildingTemplates.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _freeBuildingTemplates = null;

        public int FreeBuildingCount {
            get { return _freeBuildingCount; }
        }
        [SerializeField] private int _freeBuildingCount = 0;

        public IProductionModifier ProductionModifier {
            get { return _productionModifier; }
        }
        [SerializeField] private ProductionModifier _productionModifier = null;

        public IEnumerable<IUnitTemplate> FreeUnits {
            get { return _freeUnits.Cast<IUnitTemplate>(); }
        }
        [SerializeField] private List<UnitTemplate> _freeUnits = null;

        public float PolicyCostFromCityCountModifier {
            get { return _policyCostFromCityCountModifier; }
        }
        [SerializeField] private float _policyCostFromCityCountModifier = 0f;

        public int FreeGreatPeople {
            get { return _freeGreatPeople; }
        }
        [SerializeField] private int _freeGreatPeople = 0;

        public float ImprovementSpeedModifier {
            get { return _improvementSpeedModifier; }
        }
        [SerializeField] private float _improvementSpeedModifier = 0f;

        public float UnitExperienceGainModifier {
            get { return _unitExperienceGainModifier; }
        }
        [SerializeField] private float _unitExperienceGainModifier = 0f;

        public float GreatMilitaryGainSpeedModifier {
            get { return _greatMilitaryGainSpeedModifier; }
        }
        [SerializeField] private float _greatMilitaryGainSpeedModifier = 0f;

        public float GoldenAgeLengthModifier {
            get { return _goldenAgeLengthModifier; }
        }
        [SerializeField] private float _goldenAgeLengthModifier = 0f;

        public bool StartsGoldenAge {
            get { return _startsGoldenAge; }
        }
        [SerializeField] private bool _startsGoldenAge = false;

        public float GoldBountyPerProduction {
            get { return _goldBountyPerProduction; }
        }
        [SerializeField] private float _goldBountyPerProduction = 0f;

        public IEnumerable<IPromotion> GlobalPromotions {
            get { return _globalPromotions.Cast<IPromotion>(); }
        }
        [SerializeField] private List<Promotion> _globalPromotions = null;

        public YieldSummary GarrisonedCityYield {
            get { return _garrisonedCityYield; }
        }
        [SerializeField] private YieldSummary _garrisonedCityYield = YieldSummary.Empty;

        public int GarrisonedCityHappiness {
            get { return _garrisonedCityHappiness; }
        }
        [SerializeField] private int _garrisonedCityHappiness = 0;

        #endregion

    }

}
