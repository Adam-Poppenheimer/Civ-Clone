using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Core {

    public class GameCore {

        #region instance fields and properties

        public ICivilization PlayerCivilization { get; private set; }

        private ITurnExecuter TurnExecuter;

        private TurnBeganSignal TurnBeganSignal;
        private TurnEndedSignal TurnEndedSignal;

        private IRecordkeepingCityFactory CityFactory;
        private ICivilizationFactory CivilizationFactory;

        #endregion

        #region constructors

        [Inject]
        public GameCore(
            IRecordkeepingCityFactory cityFactory, ICivilizationFactory civilizationFactory,
            ITurnExecuter turnExecuter, TurnBeganSignal turnBeganSignal,
            TurnEndedSignal turnEndedSignal, EndTurnRequestedSignal endTurnRequestedSignal
        ){
            CityFactory = cityFactory;
            CivilizationFactory = civilizationFactory;

            TurnExecuter = turnExecuter;
            TurnBeganSignal = turnBeganSignal;
            TurnEndedSignal = turnEndedSignal;
            
            endTurnRequestedSignal.Listen(OnEndTurnRequested);

            PlayerCivilization = CivilizationFactory.Create("Player Civilization");
        }

        #endregion

        #region instance methods

        public void BeginRound() {
            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.BeginTurnOnCity(city);
            }

            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                TurnExecuter.BeginTurnOnCivilization(civilization);
            }

            TurnBeganSignal.Fire(0);
        }

        public void EndRound() {
            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.EndTurnOnCity(city);
            }

            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                TurnExecuter.EndTurnOnCivilization(civilization);
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
