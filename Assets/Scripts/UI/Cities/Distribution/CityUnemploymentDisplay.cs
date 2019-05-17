using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;

namespace Assets.UI.Cities.Distribution {

    public class CityUnemploymentDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text UnemployedPeopleField = null;

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private IUnemploymentLogic UnemploymentLogic;
        private CitySignals        CitySignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IUnemploymentLogic unemploymentLogic, CitySignals citySignals
        ) {
            UnemploymentLogic = unemploymentLogic;
            CitySignals       = citySignals;
        }

        #region from CityDisplayBase

        protected override void DoOnEnable() {
            SignalSubscriptions.Add(CitySignals.DistributionPerformed.Subscribe(data => Refresh()));
        }

        protected override void DoOnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
            SignalSubscriptions.Clear();
        }

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            UnemployedPeopleField.text = UnemploymentLogic.GetUnemployedPeopleInCity(ObjectToDisplay).ToString();
        }

        #endregion

        #endregion

    }

}
