using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Simulation.Core {

    public class GameCore {

        #region instance fields and properties

        public ICivilization PlayerCivilization { get; private set; }

        private ICityFactory CityFactory;
        private ICivilizationFactory      CivilizationFactory;
        private IUnitFactory              UnitFactory;

        private ITurnExecuter TurnExecuter;

        private TurnBeganSignal TurnBeganSignal;
        private TurnEndedSignal TurnEndedSignal;        

        #endregion

        #region constructors

        [Inject]
        public GameCore(
            ICityFactory cityFactory, ICivilizationFactory civilizationFactory,
            IUnitFactory unitFactory, ITurnExecuter turnExecuter, TurnBeganSignal turnBeganSignal,
            TurnEndedSignal turnEndedSignal, EndTurnRequestedSignal endTurnRequestedSignal
        ){
            CityFactory         = cityFactory;
            CivilizationFactory = civilizationFactory;
            UnitFactory         = unitFactory;

            TurnExecuter = turnExecuter;

            TurnBeganSignal = turnBeganSignal;
            TurnEndedSignal = turnEndedSignal;
            
            endTurnRequestedSignal.Listen(OnEndTurnRequested);

            PlayerCivilization = CivilizationFactory.Create("Player Civilization");
        }

        #endregion

        #region instance methods

        public void BeginRound() {
            foreach(var unit in UnitFactory.AllUnits) {
                TurnExecuter.BeginTurnOnUnit(unit);
            }

            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.BeginTurnOnCity(city);
            }

            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                TurnExecuter.BeginTurnOnCivilization(civilization);
            }

            TurnBeganSignal.Fire(0);
        }

        public void EndRound() {
            foreach(var unit in UnitFactory.AllUnits) {
                TurnExecuter.EndTurnOnUnit(unit);
            }

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
