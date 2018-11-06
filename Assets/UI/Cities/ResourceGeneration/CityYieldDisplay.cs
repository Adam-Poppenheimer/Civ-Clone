using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.UI.Cities.ResourceGeneration {

    public class CityYieldDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private YieldSummaryDisplay YieldDisplay;

        private IDisposable DistributionPerformedSubscription;



        private IYieldGenerationLogic YieldGenerationLogic;
        private CitySignals           CitySignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IYieldGenerationLogic resourceGenerationLogic, CitySignals citySignals
        ) {
            YieldGenerationLogic = resourceGenerationLogic;
            CitySignals          = citySignals;
        }

        #region from CityDisplayBase

        protected override void DoOnEnable() {
            DistributionPerformedSubscription = CitySignals.DistributionPerformedSignal.Subscribe(OnDistributionPerformed);
        }

        protected override void DoOnDisable() {
            DistributionPerformedSubscription.Dispose();
        }

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            YieldDisplay.DisplaySummary(YieldGenerationLogic.GetTotalYieldForCity(ObjectToDisplay));
        }

        #endregion

        private void OnDistributionPerformed(ICity city) {
            if(ObjectToDisplay == city) {
                Refresh();
            }
        }

        #endregion

    }

}
