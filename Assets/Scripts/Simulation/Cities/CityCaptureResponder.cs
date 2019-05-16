using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.UI.Cities;

namespace Assets.Simulation.Cities {

    public class CityCaptureResponder {

        #region instance fields and properties

        private ICityCaptureDisplay CityCaptureDisplay;
        private CitySignals         CitySignals;
        private Animator            UIAnimator;

        #endregion

        #region constructors

        [Inject]
        public CityCaptureResponder(
            ICityCaptureDisplay cityCaptureDisplay, CitySignals citySignals,
            [Inject(Id = "UI Animator")] Animator uiAnimator
        ) {
            CityCaptureDisplay = cityCaptureDisplay;
            UIAnimator         = uiAnimator;

            citySignals.CityCaptured.Subscribe(OnCityCaptured);
        }

        #endregion

        #region instance methods

        private void OnCityCaptured(CityCaptureData captureData) {
            CityCaptureDisplay.TargetedCity = captureData.City;

            UIAnimator.SetTrigger("City Capture Display Requested");
        }

        #endregion

    }

}
