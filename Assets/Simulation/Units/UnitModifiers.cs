using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Units {

    public class UnitModifiers : IUnitModifiers {

        #region instance fields and properties

        #region from IUnitModifiers

        public IUnitModifier<float> ExperienceGain { get; private set; }

        #endregion

        #endregion

        #region constructors

        [Inject]
        public UnitModifiers(DiContainer container) {
            ExperienceGain = new UnitModifier<float>(
                new UnitModifier<float>.ExtractionData() {
                    PolicyBonusesExtractor = bonuses => bonuses.UnitExperienceGainModifier,
                    Aggregator = (a, b) => a + b,
                    UnitaryValue = 1f
                }
            );

            container.QueueForInject(ExperienceGain);
        }

        #endregion

    }

}
