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
using Assets.Simulation.HexMap;

using Assets.UI.Core;

namespace Assets.Simulation.Core {

    /// <summary>
    /// Controls chronological progression and other core elements of the game. Currently, that means handling
    /// turn incrementation and keeping a reference to the player civilization.
    /// </summary>
    public class GameCore : IGameCore {

        #region instance fields and properties

        /// <summary>
        /// The civilization the player controls.
        /// </summary>
        public ICivilization ActiveCivilization { get; set; }

        public int CurrentRound { get; private set; }

        private ICityFactory         CityFactory;
        private ICivilizationFactory CivilizationFactory;
        private IUnitFactory         UnitFactory;
        private IAbilityExecuter     AbilityExecuter;

        private IRoundExecuter TurnExecuter;

        private CoreSignals CoreSignals;

        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public GameCore(
            ICityFactory cityFactory, ICivilizationFactory civilizationFactory,
            IUnitFactory unitFactory, IAbilityExecuter abilityExecuter,
            IRoundExecuter turnExecuter, CoreSignals coreSignals, IHexGrid grid,
            PlayerSignals playerSignals,  CivilizationSignals civSignals
            
        ){
            CityFactory         = cityFactory;
            CivilizationFactory = civilizationFactory;
            UnitFactory         = unitFactory;
            AbilityExecuter     = abilityExecuter;
            TurnExecuter        = turnExecuter;
            CoreSignals         = coreSignals;
            Grid                = grid;
            
            playerSignals.EndTurnRequestedSignal.Subscribe(OnEndTurnRequested);

            civSignals.CivilizationBeingDestroyedSignal.Subscribe(OnCivilizationBeingDestroyed);
            civSignals.NewCivilizationCreatedSignal    .Subscribe(OnNewCivilizationCreated);
        }

        #endregion

        #region instance methods

        #region from IGameCore

        public void EndTurn() {
            if(ActiveCivilization == null) {
                return;
            }

            PerformEndOfTurnActions();

            var allCivs = CivilizationFactory.AllCivilizations;

            if(ActiveCivilization == allCivs.Last()) {
                EndRound();
                BeginRound();
                ActiveCivilization = allCivs.First();
            }else {
                ActiveCivilization = allCivs[allCivs.IndexOf(ActiveCivilization) + 1];
            }

            PerformBeginningOfTurnActions();
        }
        
        public void BeginRound() {
            foreach(var unit in UnitFactory.AllUnits) {
                TurnExecuter.BeginRoundOnUnit(unit);
            }

            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.BeginRoundOnCity(city);
            }

            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                TurnExecuter.BeginRoundOnCivilization(civilization);
            }

            CoreSignals.RoundBeganSignal.OnNext(++CurrentRound);
        }

        public void EndRound() {
            foreach(var unit in UnitFactory.AllUnits) {
                TurnExecuter.EndRoundOnUnit(unit);
            }

            foreach(var city in CityFactory.AllCities) {
                TurnExecuter.EndRoundOnCity(city);
            }

            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                TurnExecuter.EndRoundOnCivilization(civilization);
            }

            AbilityExecuter.PerformOngoingAbilities();

            CoreSignals.RoundEndedSignal.OnNext(CurrentRound);
        }

        #endregion

        private void PerformBeginningOfTurnActions() {
            foreach(var cell in Grid.AllCells) {
                cell.RefreshVisibility();
            }
        }

        private void PerformEndOfTurnActions() {

        }

        private void OnEndTurnRequested(Unit unit) {
            EndTurn();
        }

        private void OnNewCivilizationCreated(ICivilization civ) {
            if(ActiveCivilization == null) {
                ActiveCivilization = civ;
            }
        }

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            if(ActiveCivilization == civ) {
                if(CivilizationFactory.AllCivilizations.Count > 0) {
                    EndTurn();
                }else {
                    ActiveCivilization = null;
                }
            }
        }

        #endregion

    }

}
