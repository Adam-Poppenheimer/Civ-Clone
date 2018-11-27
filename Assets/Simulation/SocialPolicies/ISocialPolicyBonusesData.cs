using System;
using System.Collections.Generic;

using Assets.Simulation.Modifiers;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.SocialPolicies {

    public interface ISocialPolicyBonusesData {

        #region properties

        YieldSummary CapitalYield { get; }
        YieldSummary CityYield    { get; }

        float CapitalBorderExpansionModifier { get; }
        float CityBorderExpansionModifier    { get; }

        float CapitalGrowthModifier { get; }
        float CityGrowthModifier    { get; }

        float CityWonderProductionModifier { get; }

        float CapitalHappinessPerPopulation { get; }
        float CityHappinessPerPopulation    { get; }

        float CapitalUnhappinessPerPopulation { get; }
        float CityUnhappinessPerPopulation    { get; }

        bool SuppressesGarrisonedUnitMaintenance { get; }

        float GarrisonedCityBonusStrength { get; }

        IEnumerable<IBuildingTemplate> FreeBuildingTemplates { get; }
        int                            FreeBuildingCount     { get; }

        #endregion

    }

}