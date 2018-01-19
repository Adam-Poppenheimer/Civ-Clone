using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities.Territory {

    /// <summary>
    /// The standard implementation of ICellPossessionCanon.
    /// </summary>
    public class CellPossessionCanon : PossessionRelationship<ICity, IHexCell> {

        #region constructors

        [Inject]
        public CellPossessionCanon(CitySignals citySignals) {
            citySignals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICity, IHexCell>

        protected override bool IsPossessionValid(IHexCell possession, ICity owner) {
            if(owner == null) {
                return true;
            }else {
                var currentOwner = GetOwnerOfPossession(possession);
                return currentOwner == null || currentOwner.Location != possession;
            }
        }

        protected override void DoOnPossessionBroken(IHexCell possession, ICity oldOwner) {
            possession.Refresh();
        }

        protected override void DoOnPossessionEstablished(IHexCell possession, ICity newOwner) {
            possession.Refresh();
        }

        #endregion

        private void OnCityBeingDestroyed(ICity city) {
            foreach(var cell in new List<IHexCell>(GetPossessionsOfOwner(city))) {
                ChangeOwnerOfPossession(cell, null);
            }
        }

        #endregion
        
    }

}
