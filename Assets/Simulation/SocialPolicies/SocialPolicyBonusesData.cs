﻿using System;
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

        public int ConnectedToCapitalHappiness {
            get { return _connectedToCapitalHappiness; }
        }
        [SerializeField] private int _connectedToCapitalHappiness;

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

        public int FreeGreatPeople {
            get { return _freeGreatPeople; }
        }
        [SerializeField] private int _freeGreatPeople;

        public float ImprovementSpeedModifier {
            get { return _improvementSpeedModifier; }
        }
        [SerializeField] private float _improvementSpeedModifier;

        public float UnitExperienceGainModifier {
            get { return _unitExperienceGainModifier; }
        }
        [SerializeField] private float _unitExperienceGainModifier;

        public float GreatMilitaryGainSpeedModifier {
            get { return _greatMilitaryGainSpeedModifier; }
        }
        [SerializeField] private float _greatMilitaryGainSpeedModifier;

        public float GoldenAgeLengthModifier {
            get { return _goldenAgeLengthModifier; }
        }
        [SerializeField] private float _goldenAgeLengthModifier;

        public bool StartsGoldenAge {
            get { return _startsGoldenAge; }
        }
        [SerializeField] private bool _startsGoldenAge;

        public float GoldBountyPerProduction {
            get { return _goldBountyPerProduction; }
        }
        [SerializeField] private float _goldBountyPerProduction;

        public IEnumerable<IPromotion> GlobalPromotions {
            get { return _globalPromotions.Cast<IPromotion>(); }
        }
        [SerializeField] private List<Promotion> _globalPromotions;

        public YieldSummary GarrisonedCityYield {
            get { return _garrisonedCityYield; }
        }
        [SerializeField] private YieldSummary _garrisonedCityYield;

        public int GarrisonedCityHappiness {
            get { return _garrisonedCityHappiness; }
        }
        [SerializeField] private int _garrisonedCityHappiness;

        #endregion

    }

}