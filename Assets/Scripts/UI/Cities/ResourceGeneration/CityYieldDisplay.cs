﻿using System;
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

        [SerializeField] private YieldSummaryDisplay NormalYieldDisplay      = null;
        [SerializeField] private YieldSummaryDisplay GreatPersonYieldDisplay = null;

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();



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
            SignalSubscriptions.Add(CitySignals.DistributionPerformed.Subscribe(OnDistributionPerformed));
            SignalSubscriptions.Add(CitySignals.GainedBuilding   .Subscribe(data => Refresh()));            
            SignalSubscriptions.Add(CitySignals.LostBuilding     .Subscribe(data => Refresh()));
        }

        protected override void DoOnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
            SignalSubscriptions.Clear();
        }

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            var cityYield = YieldGenerationLogic.GetTotalYieldForCity(ObjectToDisplay);

            NormalYieldDisplay     .DisplaySummary(cityYield);
            GreatPersonYieldDisplay.DisplaySummary(cityYield);
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
