using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    public class CityLocationCanon : PossessionRelationship<IHexCell, ICity> {

        #region instance fields and properties

        private CitySignals CitySignals;

        #endregion

        #region constructors

        [Inject]
        public CityLocationCanon(CitySignals citySignals, HexCellSignals cellSignals) {
            CitySignals = citySignals;

            citySignals.BeingDestroyed.Subscribe(OnCityBeingDestroyed);

            cellSignals.MapBeingClearedSignal.Subscribe(unit => Clear(false));
        }

        #endregion

        #region instance methods

        #region from PossesionRelationship<IHexCell, ICity>

        protected override void DoOnPossessionEstablished(ICity possession, IHexCell newOwner) {
            CitySignals.CityAddedToLocation.OnNext(new Tuple<ICity, IHexCell>(possession, newOwner));
        }

        protected override void DoOnPossessionBroken(ICity possession, IHexCell oldOwner) {
            CitySignals.CityRemovedFromLocation.OnNext(new Tuple<ICity, IHexCell>(possession, oldOwner));
        }

        #endregion

        private void OnCityBeingDestroyed(ICity city) {
            var oldOwner = GetOwnerOfPossession(city);

            if(oldOwner != null) {
                ChangeOwnerOfPossession(city, null);
            }
        }

        #endregion

    }

}
