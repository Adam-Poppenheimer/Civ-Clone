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
        public CityPossessionResponder(CivilizationSignals civSignals,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;

            civSignals.CivGainedCitySignal.Subscribe(OnCityOwnershipChanged);
        }

        #endregion

        #region instance methods

        private void OnCityOwnershipChanged(Tuple<ICivilization, ICity> data) {
            var newOwner = data.Item1;
            var city     = data.Item2;

            UnitPossessionCanon.ChangeOwnerOfPossession(city.CombatFacade, newOwner);
        }

        #endregion

    }

}
