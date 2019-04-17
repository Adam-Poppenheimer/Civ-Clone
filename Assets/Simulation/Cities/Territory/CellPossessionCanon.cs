using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities.Territory {

    /// <summary>
    /// The standard implementation of ICellPossessionCanon.
    /// </summary>
    public class CellPossessionCanon : PossessionRelationship<ICity, IHexCell> {

        #region instance fields and properties

        private CitySignals                              CitySignals;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CellPossessionCanon(
            CitySignals citySignals, HexCellSignals cellSignals,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            CitySignals       = citySignals;
            CityLocationCanon = cityLocationCanon;

            citySignals.BeingDestroyed.Subscribe(OnCityBeingDestroyed);

            cellSignals.MapBeingClearedSignal.Subscribe(unit => Clear(false));
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICity, IHexCell>

        protected override bool IsPossessionValid(IHexCell possession, ICity owner) {
            if(owner == null) {
                return true;
            }else {
                var citiesAtLocation = CityLocationCanon.GetPossessionsOfOwner(possession);

                return !citiesAtLocation.Any() || citiesAtLocation.Contains(owner);
            }
        }

        protected override void DoOnPossessionBroken(IHexCell possession, ICity oldOwner) {
            if(oldOwner != null) {
                CitySignals.LostCellFromBoundaries.OnNext(new UniRx.Tuple<ICity, IHexCell>(oldOwner, possession));
                oldOwner.PerformDistribution();
            }

            possession.RefreshVisibility();
        }

        protected override void DoOnPossessionEstablished(IHexCell possession, ICity newOwner) {
            if(newOwner != null) {
                CitySignals.GainedCellToBoundaries.OnNext(new UniRx.Tuple<ICity, IHexCell>(newOwner, possession));
                newOwner.PerformDistribution();
            }

            possession.RefreshVisibility();
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
