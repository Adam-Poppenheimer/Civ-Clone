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

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CityPossessionCanon(
            CitySignals citySignals, CivilizationSignals civSignals,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon
        ) {
            citySignals.CityBeingDestroyedSignal        .Subscribe(OnCityBeingDestroyed);
            civSignals .CivilizationBeingDestroyedSignal.Subscribe(OnCivilizationBeingDestroyed);

            CellPossessionCanon = cellPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICivilization, ICity>

        protected override void DoOnPossessionEstablished(ICity possession, ICivilization newOwner) {
            foreach(var cell in CellPossessionCanon.GetPossessionsOfOwner(possession)) {
                cell.RefreshSelfOnly();
            }
        }

        #endregion

        private void OnCityBeingDestroyed(ICity city) {
            ChangeOwnerOfPossession(city, null);
        }

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            foreach(var city in GetPossessionsOfOwner(civ)) {
                GameObject.Destroy(city.gameObject.gameObject);
            }
        }

        #endregion

    }

}
