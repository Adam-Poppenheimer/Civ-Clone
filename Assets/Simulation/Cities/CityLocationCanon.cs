using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    public class CityLocationCanon : PossessionRelationship<IHexCell, ICity> {

        #region constructors

        [Inject]
        public CityLocationCanon(CitySignals signals) {
            signals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossesionRelationship<IHexCell, ICity>

        protected override void DoOnPossessionEstablished(ICity possession, IHexCell newOwner) {
            newOwner.RefreshSelfOnly();
        }

        protected override void DoOnPossessionBroken(ICity possession, IHexCell oldOwner) {
            oldOwner.RefreshSelfOnly();
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
