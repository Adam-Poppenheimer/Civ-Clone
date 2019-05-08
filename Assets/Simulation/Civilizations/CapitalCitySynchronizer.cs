using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class CapitalCitySynchronizer : ICapitalCitySynchronizer {

        #region instance fields and properties

        #region from ICapitalCitySynchronizer

        public bool IsUpdatingCapitals { get; private set; }

        #endregion

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private ICapitalCityCanon                             CapitalCityCanon;
        private CivilizationSignals                           CivSignals;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CapitalCitySynchronizer(
            ICapitalCityCanon capitalCityCanon, CivilizationSignals civSignals,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            CapitalCityCanon    = capitalCityCanon;
            CivSignals          = civSignals;
            CityPossessionCanon = cityPossessionCanon;

            SetCapitalUpdating(true);
        }

        #endregion

        #region instance methods

        #region from ICapitalCitySynchronizer

        public void SetCapitalUpdating(bool updateCapitals) {
            if(IsUpdatingCapitals != updateCapitals) {
                IsUpdatingCapitals = updateCapitals;

                if(IsUpdatingCapitals) {
                    SignalSubscriptions.Add(CivSignals.CivGainedCity.Subscribe(OnCivGainedCity));
                    SignalSubscriptions.Add(CivSignals.CivLostCity  .Subscribe(OnCivLostCity));
                }else {
                    SignalSubscriptions.ForEach(subscription => subscription.Dispose());
                    SignalSubscriptions.Clear();
                }
            }
        }

        #endregion

        private void OnCivGainedCity(UniRx.Tuple<ICivilization, ICity> data) {
            var civ  = data.Item1;
            var city = data.Item2;

            if(CapitalCityCanon.GetCapitalOfCiv(civ) == null) {
                CapitalCityCanon.SetCapitalOfCiv(civ, city);
            }
        }

        private void OnCivLostCity(UniRx.Tuple<ICivilization, ICity> data) {
            var civ  = data.Item1;
            var city = data.Item2;

            if(city == CapitalCityCanon.GetCapitalOfCiv(civ)) {
                var nextCity = CityPossessionCanon.GetPossessionsOfOwner(civ).FirstOrDefault();

                CapitalCityCanon.SetCapitalOfCiv(civ, nextCity);
            }
        }

        #endregion

    }

}
