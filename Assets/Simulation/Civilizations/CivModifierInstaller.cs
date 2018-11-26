using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Modifiers;

namespace Assets.Simulation.Civilizations {

    public class CivModifierInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            var suppressGarrisionMaintenanceModifier = new CivModifier<bool>(
                new CivModifier<bool>.ExtractionData() {
                    Extractor    = bonuses => bonuses.SuppressesGarrisionedUnitMaintenance,
                    Aggregator   = (a, b)  => a || b,
                    UnitaryValue = false
                }
            );
            Container.Bind<ICivModifier<bool>>().WithId("Suppress Garrision Maintenance Modifier")
                     .To<   CivModifier<bool>>().FromInstance(suppressGarrisionMaintenanceModifier);
            Container.QueueForInject(suppressGarrisionMaintenanceModifier);
        }

        #endregion

        #endregion

    }

}
