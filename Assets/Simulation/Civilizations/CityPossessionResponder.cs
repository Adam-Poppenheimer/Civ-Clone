using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class CityPossessionResponder {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CityPossessionResponder(CitySignals citySignals,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;

            citySignals.OwnershipChangedSignal.Subscribe(OnCityOwnershipChanged);
        }

        #endregion

        #region instance methods

        private void OnCityOwnershipChanged(Tuple<ICity, ICivilization> data) {
            var city = data.Item1;
            var newOwner = data.Item2;

            UnitPossessionCanon.ChangeOwnerOfPossession(city.CombatFacade, newOwner);
        }

        #endregion

    }

}
