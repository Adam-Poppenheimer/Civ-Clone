using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Civilizations {

    public class CityPossessionCanon : PossessionRelationship<ICivilization, ICity> {

        #region instance fields and properties

        private CivilizationSignals CivSignals;

        #endregion

        #region constructors

        [Inject]
        public CityPossessionCanon(
            CitySignals citySignals, CivilizationSignals civSignals,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            HexCellSignals cellSignals
        ) {
            CivSignals = civSignals;

            citySignals.BeingDestroyed   .Subscribe(OnCityBeingDestroyed);
            civSignals .CivBeingDestroyed.Subscribe(OnCivilizationBeingDestroyed);

            cellSignals.MapBeingClearedSignal.Subscribe(unit => Clear(false));
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICivilization, ICity>

        protected override void DoOnPossessionBeingBroken(ICity possession, ICivilization oldOwner) {
            CivSignals.CivLosingCity.OnNext(new UniRx.Tuple<ICivilization, ICity>(oldOwner, possession));
        }

        protected override void DoOnPossessionEstablished(ICity city, ICivilization newOwner) {
            if(newOwner != null) {
                CivSignals.CivGainedCity.OnNext(new UniRx.Tuple<ICivilization, ICity>(newOwner, city));
            }
        }

        protected override void DoOnPossessionBroken(ICity city, ICivilization oldOwner) {
            CivSignals.CivLostCity.OnNext(new UniRx.Tuple<ICivilization, ICity>(oldOwner, city));
        }

        #endregion

        private void OnCityBeingDestroyed(ICity city) {
            ChangeOwnerOfPossession(city, null);
        }

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            foreach(var city in GetPossessionsOfOwner(civ)) {
                city.Destroy();
            }
        }

        #endregion

    }

}
