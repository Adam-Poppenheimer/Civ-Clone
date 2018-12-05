using System;
using System.Collections.Generic;

using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.SocialPolicies {

    public interface ISocialPolicyBonusesData {

        #region properties

        YieldSummary CapitalYield { get; }
        YieldSummary CityYield    { get; }

        float CapitalBorderExpansionModifier { get; }
        float CityBorderExpansionModifier    { get; }

        float CapitalGrowthModifier { get; }
        float CityGrowthModifier    { get; }

        float CapitalHappinessPerPopulation { get; }
        float CityHappinessPerPopulation    { get; }

        float CapitalUnhappinessPerPopulation { get; }
        float CityUnhappinessPerPopulation    { get; }

        int ConnectedToCapitalHappiness { get; }

        bool SuppressesGarrisonedUnitMaintenance { get; }

        float GarrisonedCityBonusStrength { get; }

        IEnumerable<IBuildingTemplate> FreeBuildingTemplates { get; }
        int                            FreeBuildingCount     { get; }

        IProductionModifier ProductionModifier { get; }

        IEnumerable<IUnitTemplate> FreeUnits { get; }

        float PolicyCostFromCityCountModifier { get; }

        int FreeGreatPeople { get; }

        float ImprovementSpeedModifier { get; }

        float UnitExperienceGainModifier { get; }

        float GreatMilitaryGainSpeedModifier { get; }

        float GoldenAgeLengthModifier { get; }

        bool StartsGoldenAge { get; }

        float GoldBountyPerProduction { get; }

        IEnumerable<IPromotion> GlobalPromotions { get; }

        #endregion

    }

}