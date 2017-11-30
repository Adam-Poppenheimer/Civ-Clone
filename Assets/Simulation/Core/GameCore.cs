using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Core {

    public class GameCore {

        #region instance fields and properties

        private ITurnExecuter TurnExecuter;

        private TurnBeganSignal TurnBeganSignal;
        private TurnEndedSignal TurnEndedSignal;

        private IRecordkeepingCityFactory CityFactory;

        #endregion

        #region constructors

        [Inject]
        public GameCore(
            ITurnExecuter turnExecuter, TurnBeganSignal turnBeganSignal,
            TurnEndedSignal turnEndedSignal, IRecordkeepingCityFactory cityFactory,
            EndTurnRequestedSignal endTurnRequestedSignal
        ){
            TurnExecuter = turnExecuter;
            TurnBeganSignal = turnBeganSignal;
            TurnEndedSignal = turnEndedSignal;

            CityFactory = cityFactory;
            endTurnRequestedSignal.Listen(OnEndTurnRequested);
        }

        #endregion

        #region instance methods

        public void BeginRound() {
            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.BeginTurnOnCity(city);
            }

            TurnBeganSignal.Fire(0);
        }

        public void EndRound() {
            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.EndTurnOnCity(city);
            }

            TurnEndedSignal.Fire(0);
        }

        private void OnEndTurnRequested() {
            EndRound();
            BeginRound();
        }

        #endregion

    }

}
