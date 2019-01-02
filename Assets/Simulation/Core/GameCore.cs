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
using Assets.Simulation.Players;
using Assets.Simulation.HexMap;
using Assets.Simulation.Diplomacy;

using Assets.UI.Core;

namespace Assets.Simulation.Core {

    /// <summary>
    /// Controls chronological progression and other core elements of the game. Currently, that means handling
    /// turn incrementation and keeping a reference to the player civilization.
    /// </summary>
    public class GameCore : IGameCore {

        #region instance fields and properties

        public IPlayer ActivePlayer {
            get { return _activePlayer; }
            set {
                if(_activePlayer != value) {
                    _activePlayer = value;
                    CoreSignals.ActivePlayerChangedSignal.OnNext(_activePlayer);
                }
            }
        }
        private IPlayer _activePlayer;

        public ICivilization ActiveCiv {
            get {
                if(ActivePlayer != null) {
                    return ActivePlayer.ControlledCiv;
                }else {
                    return null;
                }
            }
        }

        public int CurrentRound { get; private set; }

        private ICityFactory   CityFactory;
        private IPlayerFactory PlayerFactory;
        private IUnitFactory   UnitFactory;
        private IRoundExecuter RoundExecuter;
        private CoreSignals    CoreSignals;
        private IHexGrid       Grid;
        private IDiplomacyCore DiplomacyCore;

        #endregion

        #region constructors

        [Inject]
        public GameCore(
            ICityFactory cityFactory, IPlayerFactory playerFactory,
            IUnitFactory unitFactory, IRoundExecuter turnExecuter,
            CoreSignals coreSignals, IHexGrid grid, IDiplomacyCore diplomacyCore,
            PlayerSignals playerSignals
        ){
            CityFactory   = cityFactory;
            PlayerFactory = playerFactory;
            UnitFactory   = unitFactory;
            RoundExecuter = turnExecuter;
            CoreSignals   = coreSignals;
            Grid          = grid;
            DiplomacyCore = diplomacyCore;

            playerSignals.PlayerBeingDestroyed.Subscribe(OnPlayerBeingDestroyed);
            playerSignals.PlayerCreated       .Subscribe(OnPlayerCreated);
        }

        #endregion

        #region instance methods

        #region from IGameCore
        
        public void BeginRound() {
            foreach(var unit in UnitFactory.AllUnits) {
                RoundExecuter.BeginRoundOnUnit(unit);
            }

            foreach(var city in CityFactory.AllCities) {
                RoundExecuter.BeginRoundOnCity(city);
            }

            foreach(var player in PlayerFactory.AllPlayers) {
                RoundExecuter.BeginRoundOnCivilization(player.ControlledCiv);
            }

            CoreSignals.RoundBeganSignal.OnNext(++CurrentRound);
        }

        public void EndRound() {
            foreach(var unit in UnitFactory.AllUnits) {
                RoundExecuter.EndRoundOnUnit(unit);
            }

            foreach(var city in CityFactory.AllCities) {
                RoundExecuter.EndRoundOnCity(city);
            }

            foreach(var player in PlayerFactory.AllPlayers) {
                RoundExecuter.EndRoundOnCivilization(player.ControlledCiv);
            }

            DiplomacyCore.UpdateOngoingDeals();

            CoreSignals.RoundEndedSignal.OnNext(CurrentRound);
        }

        #endregion

        private void StartTurn(IPlayer player) {
            ActivePlayer = player;

            player.PassControl(() => EndTurn());
        }

        public void EndTurn() {
            Debug.Log("Ending turn on player " + (ActivePlayer != null ? ActivePlayer.Name : "NULL"));

            if(ActivePlayer == null) {
                return;
            }

            PerformEndOfTurnActions();

            var allPlayers = PlayerFactory.AllPlayers;

            if(ActivePlayer == allPlayers.Last()) {
                EndRound();
                BeginRound();

                StartTurn(allPlayers.First());
            }else {
                StartTurn(allPlayers[allPlayers.IndexOf(ActivePlayer) + 1]);
            }

            PerformBeginningOfTurnActions();
        }

        private void PerformBeginningOfTurnActions() {
            foreach(var cell in Grid.Cells) {
                cell.RefreshVisibility();
            }

            CoreSignals.TurnBeganSignal.OnNext(ActivePlayer);
        }

        private void PerformEndOfTurnActions() {
            CoreSignals.TurnEndedSignal.OnNext(ActivePlayer);
        }

        private void OnPlayerCreated(IPlayer player) {
            if(ActivePlayer == null) {
                StartTurn(player);
            }
        }

        private void OnPlayerBeingDestroyed(IPlayer player) {
            if(ActivePlayer == player) {
                if(PlayerFactory.AllPlayers.Count > 0) {
                    EndTurn();
                }else {
                    ActivePlayer = null;
                }
            }
        }

        #endregion

    }

}
