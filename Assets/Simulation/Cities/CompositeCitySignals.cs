using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities {

    public class CompositeCitySignals {

        #region instance fields and properties

        public IObservable<ICity> ActiveCivCityClickedSignal { get; private set; }

        private IGameCore GameCore;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        public CompositeCitySignals(CitySignals signals, IGameCore gameCore,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon            
        ) {
            GameCore            = gameCore;
            CityPossessionCanon = cityPossessionCanon;

            ActiveCivCityClickedSignal = signals.PointerClickedSignal.Where(ActivCivCityFilter);
        }

        #endregion

        #region instance methods

        private bool ActivCivCityFilter(ICity city) {
            return CityPossessionCanon.GetOwnerOfPossession(city) == GameCore.ActiveCiv;
        }

        #endregion

    }

}
