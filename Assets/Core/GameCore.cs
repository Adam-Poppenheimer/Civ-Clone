using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Cities;

namespace Assets.Core {

    public class GameCore {

        #region instance fields and properties

        private ITurnExecuter TurnExecuter;
        private IRecordkeepingCityFactory CityFactory;

        #endregion

        #region constructors

        [Inject]
        public GameCore(ITurnExecuter turnExecuter, IRecordkeepingCityFactory cityFactory) {
            TurnExecuter = turnExecuter;
            CityFactory = cityFactory;
        }

        #endregion

        #region instance methods

        public void BeginRound() {
            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.BeginTurnOnCity(city);
            }
        }

        public void EndRound() {
            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.EndTurnOnCity(city);
            }
        }

        #endregion

    }

}
