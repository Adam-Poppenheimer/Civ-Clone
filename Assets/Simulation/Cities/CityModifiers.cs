using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Modifiers;

namespace Assets.Simulation.Cities {

    public class CityModifiers : ICityModifiers {

        #region instance fields and properties

        #region from ICityHappinessModifiers

        public ICityModifier<float> Growth           { get; private set; }
        public ICityModifier<float> BorderExpansion  { get; private set; }
        public ICityModifier<float> WonderProduction { get; private set; }

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
                    CapitalBonusesExtractor = bonuses => bonuses.CapitalGrowthModifier,
                    CityBonusesExtractor    = bonuses => bonuses.CityGrowthModifier,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            BorderExpansion = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    CapitalBonusesExtractor = bonuses => bonuses.CapitalBorderExpansionModifier,
                    CityBonusesExtractor    = bonuses => bonuses.CityBorderExpansionModifier,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            WonderProduction = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    CapitalBonusesExtractor = bonuses => 0,
                    CityBonusesExtractor    = bonuses => bonuses.CityWonderProductionModifier,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            PerPopulationHappiness = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    CapitalBonusesExtractor = bonuses => bonuses.CapitalHappinessPerPopulation,
                    CityBonusesExtractor    = bonuses => bonuses.CityHappinessPerPopulation,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            PerPopulationUnhappiness = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    CapitalBonusesExtractor = bonuses => bonuses.CapitalUnhappinessPerPopulation,
                    CityBonusesExtractor    = bonuses => bonuses.CityUnhappinessPerPopulation,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            GarrisionedRangedCombatStrength = new CityModifier<float>(
                new CityModifier<float>.ExtractionData() {
                    CapitalBonusesExtractor = bonuses => 0f,
                    CityBonusesExtractor    = bonuses => bonuses.GarrisonedCityBonusStrength,
                    Aggregator              = (a, b) => a + b,
                    UnitaryValue            = 1f
                }
            );

            container.QueueForInject(Growth);
            container.QueueForInject(BorderExpansion);
            container.QueueForInject(WonderProduction);
            container.QueueForInject(PerPopulationHappiness);
            container.QueueForInject(PerPopulationUnhappiness);
            container.QueueForInject(GarrisionedRangedCombatStrength);
        }

        #endregion
        
    }

}
