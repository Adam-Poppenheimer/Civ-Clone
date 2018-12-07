using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Cities {

    public class CityModifiers : ICityModifiers {

        #region instance fields and properties

        #region from ICityModifiers

        public ICityModifier<YieldSummary> BonusYield { get; private set; }

        public ICityModifier<float> Growth              { get; private set; }
        public ICityModifier<float> BorderExpansionCost { get; private set; }

        public ICityModifier<float> PerPopulationHappiness   { get; private set; }
        public ICityModifier<float> PerPopulationUnhappiness { get; private set; }

        public ICityModifier<int> CapitalConnectionHappiness { get; private set; }

        public ICityModifier<float> GarrisionedRangedCombatStrength { get; private set; }

        public ICityModifier<YieldSummary> GarrisonedYield     { get; private set; }
        public ICityModifier<int>          GarrisonedHappiness { get; private set; }

        

        #endregion

        #endregion

        #region constructors

        [Inject]
        public CityModifiers(DiContainer container) {
            BonusYield = new CityModifier<YieldSummary>(
                new CityModifier<YieldSummary>.ExtractionData() {
                    PolicyCapitalBonusesExtractor  = bonuses => bonuses.CapitalYield,
                    PolicyCityBonusesExtractor     = bonuses => bonuses.CityYield,
                    BuildingLocalBonusesExtractor  = null,
                    BuildingGlobalBonusesExtractor = null,
                    Aggregator   = (a, b) => a + b,
                    UnitaryValue = YieldSummary.Empty
                }
            );

            Growth = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor  = bonuses  => bonuses .CapitalGrowthModifier,
                    PolicyCityBonusesExtractor     = bonuses  => bonuses .CityGrowthModifier,
                    BuildingLocalBonusesExtractor  = template => template.LocalGrowthModifier,
                    BuildingGlobalBonusesExtractor = template => template.GlobalGrowthModifier,
                    Aggregator   = (a, b) => a + b,
                    UnitaryValue = 1f
                }
            );

            BorderExpansionCost = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => bonuses.CapitalBorderExpansionModifier,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.CityBorderExpansionModifier,
                    Aggregator   = (a, b) => a + b,
                    UnitaryValue = 1f
                }
            );

            PerPopulationHappiness = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => bonuses.CapitalHappinessPerPopulation,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.CityHappinessPerPopulation,
                    Aggregator   = (a, b) => a + b,
                    UnitaryValue = 1f
                }
            );

            PerPopulationUnhappiness = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => bonuses.CapitalUnhappinessPerPopulation,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.CityUnhappinessPerPopulation,
                    Aggregator   = (a, b) => a + b,
                    UnitaryValue = 1f
                }
            );

            CapitalConnectionHappiness = new CityModifier<int>(
                new CityModifier<int>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => 0,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.ConnectedToCapitalHappiness,
                    Aggregator   = (a, b) => a + b,
                    UnitaryValue = 0
                }
            );

            GarrisionedRangedCombatStrength = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => 0f,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.GarrisonedCityBonusStrength,
                    Aggregator  = (a, b) => a + b,
                    UnitaryValue = 1f
                }
            );

            GarrisonedYield = new CityModifier<YieldSummary>(
                new CityModifier<YieldSummary>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => YieldSummary.Empty,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.GarrisonedCityYield,
                    Aggregator   = (a, b) => a + b,
                    UnitaryValue = YieldSummary.Empty
                }
            );

            GarrisonedHappiness = new CityModifier<int>(
                new CityModifier<int>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => 0,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.GarrisonedCityHappiness,
                    Aggregator   = (a, b) => a + b,
                    UnitaryValue = 0
                }
            );

            container.QueueForInject(BonusYield);
            container.QueueForInject(Growth);
            container.QueueForInject(BorderExpansionCost);
            container.QueueForInject(PerPopulationHappiness);
            container.QueueForInject(PerPopulationUnhappiness);
            container.QueueForInject(CapitalConnectionHappiness);
            container.QueueForInject(GarrisionedRangedCombatStrength);
            container.QueueForInject(GarrisonedYield);
            container.QueueForInject(GarrisonedHappiness);
        }

        #endregion
        
    }

}
