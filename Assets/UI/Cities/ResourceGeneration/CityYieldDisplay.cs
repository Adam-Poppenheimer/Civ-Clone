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

        private IResourceSummaryDisplay YieldDisplay;

        private IResourceGenerationLogic ResourceGenerationLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            [Inject(Id = "City Yield Display")] IResourceSummaryDisplay yieldDisplay,
            IResourceGenerationLogic resourceGenerationLogic) {
            YieldDisplay = yieldDisplay;
            ResourceGenerationLogic = resourceGenerationLogic;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(CityToDisplay == null) {
                return;
            }

            YieldDisplay.DisplaySummary(ResourceGenerationLogic.GetTotalYieldForCity(CityToDisplay));
        }

        #endregion

        #endregion

    }

}
