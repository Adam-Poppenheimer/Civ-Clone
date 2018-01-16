using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class CityPossessionCanon : PossessionRelationship<ICivilization, ICity> {

        #region constructors

        [Inject]
        public CityPossessionCanon(CitySignals citySignals) {
            citySignals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);
        }

        #endregion

        #region instance methods

        private void OnCityBeingDestroyed(ICity city) {
            ChangeOwnerOfPossession(city, null);
        }

        #endregion

    }

}
