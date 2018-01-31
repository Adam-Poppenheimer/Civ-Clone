using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

using Assets.UI.Core;

namespace Assets.Simulation.Core {

    /// <summary>
    /// Controls the chronological progression and other core elements of the game. Currently, that means handling
    /// turn incrementation and keeping a reference to the player civilization.
    /// </summary>
    public class GameCore : IGameCore {

        #region instance fields and properties

        /// <summary>
        /// The civilization the player controls.
        /// </summary>
        public ICivilization ActiveCivilization { get; set; }

        private ICityFactory         CityFactory;
        private ICivilizationFactory CivilizationFactory;
        private IUnitFactory         UnitFactory;
        private IAbilityExecuter     AbilityExecuter;

        private ITurnExecuter TurnExecuter;

        private TurnBeganSignal TurnBeganSignal;
        private TurnEndedSignal TurnEndedSignal;        

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cityFactory"></param>
        /// <param name="civilizationFactory"></param>
        /// <param name="unitFactory"></param>
        /// <param name="abilityExecuter"></param>
        /// <param name="turnExecuter"></param>
        /// <param name="turnBeganSignal"></param>
        /// <param name="turnEndedSignal"></param>
        /// <param name="endTurnRequestedSignal"></param>
        [Inject]
        public GameCore(
            ICityFactory cityFactory, ICivilizationFactory civilizationFactory,
            IUnitFactory unitFactory, IAbilityExecuter abilityExecuter,
            ITurnExecuter turnExecuter, TurnBeganSignal turnBeganSignal,
            TurnEndedSignal turnEndedSignal, PlayerSignals playerSignals
        ){
            CityFactory         = cityFactory;
            CivilizationFactory = civilizationFactory;
            UnitFactory         = unitFactory;
            AbilityExecuter     = abilityExecuter;

            TurnExecuter = turnExecuter;

            TurnBeganSignal = turnBeganSignal;
            TurnEndedSignal = turnEndedSignal;
            
            playerSignals.EndTurnRequestedSignal.Subscribe(OnEndTurnRequested);

            ActiveCivilization = CivilizationFactory.Create("Player Civilization", Color.red);
        }

        #endregion

        #region instance methods

        /// <summary>
        /// Performs all activites that should happen at the beginning of a new round.
        /// </summary>
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

        /// <summary>
        /// Performs all activities that should happen at the end of a round.
        /// </summary>
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

            AbilityExecuter.PerformOngoingAbilities();

            TurnEndedSignal.Fire(0);
        }

        private void OnEndTurnRequested(Unit unit) {
            EndRound();
            BeginRound();
        }

        #endregion

    }

}
