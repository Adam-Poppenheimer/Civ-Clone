using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Civilizations {

    public class CivModifiers : ICivModifiers {

        #region instance fields and properties

        #region from ICivModifiers

        public ICivModifier<bool> SuppressGarrisonMaintenance { get; private set; }

        public ICivModifier<float> ImprovementBuildSpeed { get; private set; }

        #endregion

        #endregion

        #region constructors

        [Inject]
        public CivModifiers(DiContainer container) {
            SuppressGarrisonMaintenance = new CivModifier<bool>(
                new CivModifier<bool>.ExtractionData() {
                    PolicyExtractor         = bonuses => bonuses.SuppressesGarrisonedUnitMaintenance,
                    GlobalBuildingExtractor = null,
                    Aggregator = (a, b) => a || b,
                    UnitaryValue = false
                }
            );

            ImprovementBuildSpeed = new CivModifier<float>(
                new CivModifier<float>.ExtractionData() {
                    PolicyExtractor         = bonuses  => bonuses.ImprovementSpeedModifier,
                    GlobalBuildingExtractor = template => template.GlobalImprovementSpeedModifier,
                    Aggregator = (a, b) => a + b,
                    UnitaryValue = 1f
                }
            );

            container.QueueForInject(SuppressGarrisonMaintenance);
            container.QueueForInject(ImprovementBuildSpeed);
        }

        #endregion
        
    }

}
