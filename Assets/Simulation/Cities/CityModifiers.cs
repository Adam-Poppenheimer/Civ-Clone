using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Modifiers;

namespace Assets.Simulation.Cities {

    public class CityModifiers : ICityModifiers {

        #region instance fields and properties

        #region from ICityModifiers

        public ICityModifier<float> Growth           { get; private set; }
        public ICityModifier<float> BorderExpansion  { get; private set; }

        public ICityModifier<float> PerPopulationHappiness   { get; private set; }
        public ICityModifier<float> PerPopulationUnhappiness { get; private set; }

        public ICityModifier<float> GarrisionedRangedCombatStrength { get; private set; }

        #endregion

        #endregion

        #region constructors

        [Inject]
        public CityModifiers(DiContainer container) {
            Growth = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor  = bonuses  => bonuses .CapitalGrowthModifier,
                    PolicyCityBonusesExtractor     = bonuses  => bonuses .CityGrowthModifier,
                    BuildingLocalBonusesExtractor  = template => template.LocalGrowthModifier,
                    BuildingGlobalBonusesExtractor = template => template.GlobalGrowthModifier,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            BorderExpansion = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => bonuses.CapitalBorderExpansionModifier,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.CityBorderExpansionModifier,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            PerPopulationHappiness = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => bonuses.CapitalHappinessPerPopulation,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.CityHappinessPerPopulation,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            PerPopulationUnhappiness = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => bonuses.CapitalUnhappinessPerPopulation,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.CityUnhappinessPerPopulation,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            GarrisionedRangedCombatStrength = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => 0f,
                    PolicyCityBonusesExtractor    = bonuses => bonuses.GarrisonedCityBonusStrength,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            container.QueueForInject(Growth);
            container.QueueForInject(BorderExpansion);
            container.QueueForInject(PerPopulationHappiness);
            container.QueueForInject(PerPopulationUnhappiness);
            container.QueueForInject(GarrisionedRangedCombatStrength);
        }

        #endregion
        
    }

}
