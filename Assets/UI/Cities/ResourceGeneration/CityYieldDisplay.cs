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

        [SerializeField] private ResourceSummaryDisplay YieldDisplay;




        private IResourceGenerationLogic ResourceGenerationLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IResourceGenerationLogic resourceGenerationLogic) {
            ResourceGenerationLogic = resourceGenerationLogic;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            YieldDisplay.DisplaySummary(ResourceGenerationLogic.GetTotalYieldForCity(ObjectToDisplay));
        }

        #endregion

        #endregion

    }

}
