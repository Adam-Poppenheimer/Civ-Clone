using System;

using Assets.Simulation.Modifiers;

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

        bool SuppressesGarrisionedUnitMaintenance { get; }

        float GarrisionedCityBonusStrength { get; }

        #endregion

    }

}