using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// The base interface for the city config object, which provides various
    /// options for configuring cities.
    /// </summary>
    public interface ICityConfig {

        #region properties

        /// <summary>
        /// The maximum distance from which a city can acquire new tiles.
        /// </summary>
        int MaxBorderRange { get; }

        /// <summary>
        /// The base cost of all tiles.
        /// Used by <ref>BorderExpansionLogic.GetCultureCostOfAcquiringTile</ref>
        /// </summary>
        int TileCostBase { get; }

        /// <summary>
        /// A coefficient that modifies the cost of acquiring new tiles.
        /// Used by <ref>BorderExpansionLogic.GetCultureCostOfAcquiringTile</ref>
        /// </summary>
        int PreviousTileCountCoefficient { get; }

        /// <summary>
        /// An exponent that modifies the cost of acquiring new tiles.
        /// Used by <ref>BorderExpansionLogic.GetCultureCostOfAcquiringTile</ref>
        /// </summary>
        float PreviousTileCountExponent { get; }

        /// <summary>
        /// The yield of all unemployed citizens.
        /// </summary>
        ResourceSummary UnemployedYield { get; }

        /// <summary>
        /// The amount of gold it costs per production to hurry a project.
        /// Currently unused.
        /// </summary>
        float HurryCostPerProduction { get; }

        /// <summary>
        /// The amount of food that each citizen consumes every turn.
        /// </summary>
        int FoodConsumptionPerPerson { get; }

        /// <summary>
        /// The base amount of food required to grow a city's population.
        /// Used by <ref>PopulationGrowthLogic.GetFoodStockpileToGrow</ref>
        /// </summary>
        int BaseGrowthStockpile { get; }

        /// <summary>
        /// A coefficient that modifies the food required to grow a city's population.
        /// Used by <ref>PopulationGrowthLogic.GetFoodStockpileToGrow</ref>
        /// </summary>
        int GrowthPreviousPopulationCoefficient { get; }

        /// <summary>
        /// An exponent that modifies the food required to grow a city's population.
        /// Used by <ref>PopulationGrowthLogic.GetFoodStockpileToGrow</ref>
        /// </summary>
        float GrowthPreviousPopulationExponent { get; }

        /// <summary>
        /// The minimum distance that must be maintained between any two cities.
        /// </summary>
        int MinimumSeparation { get; }

        int CityAttackRange { get; }

        int BaseCombatStrength { get; }

        float CombatStrengthPerPopulation { get; }

        int BaseMaxHitPoints { get; }

        float MaxHitPointsPerPopulation { get; }

        int BaseRangedAttackStrength { get; }

        float RangedAttackStrengthPerPopulation { get; }

        int HitPointRegenPerRound { get; }

        int VisionRange { get; }

        int BaseHealth { get; }

        int BaseHappiness { get; }

        #endregion

    }

}
