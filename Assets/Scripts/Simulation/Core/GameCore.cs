using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Players;
using Assets.Simulation.Diplomacy;

using Assets.UI.Core;

namespace Assets.Simulation.Core {

    /// <summary>
    /// Controls chronological progression and other core elements of the game. Currently, that means handling
    /// turn incrementation and keeping a reference to the player civilization.
    /// </summary>
    public class GameCore : IGameCore {

        #region instance fields and properties

        #region from IGameCore

        public IPlayer ActivePlayer {
            get { return _activePlayer; }
            set {
                if(_activePlayer != value) {
                    _activePlayer = value;
                    CoreSignals.ActivePlayerChanged.OnNext(_activePlayer);
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

        #endregion




        
        private IPlayerFactory PlayerFactory;
        private CoreSignals    CoreSignals;

        #endregion

        #region constructors

        [Inject]
        public GameCore(
            IPlayerFactory playerFactory, CoreSignals coreSignals, PlayerSignals playerSignals
        ){
            PlayerFactory = playerFactory;
            CoreSignals   = coreSignals;

            playerSignals.PlayerBeingDestroyed.Subscribe(OnPlayerBeingDestroyed);
            playerSignals.PlayerCreated       .Subscribe(OnPlayerCreated);
        }

        #endregion

        #region instance methods

        #region from IGameCore
        
        public void BeginRound() {
            CoreSignals.StartingRound.OnNext(++CurrentRound);

            CoreSignals.RoundBegan.OnNext(CurrentRound);
        }

        public void EndRound() {
            CoreSignals.EndingRound.OnNext(CurrentRound);

            CoreSignals.RoundEnded.OnNext(CurrentRound);
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

        #endregion

        private void StartTurn(IPlayer player) {
            Debug.Log("Starting turn on player " + player.ControlledCiv.Template.Name);

            ActivePlayer = player;

            player.PassControl(() => EndTurn());
        }

        private void PerformBeginningOfTurnActions() {
            CoreSignals.TurnBegan.OnNext(ActivePlayer);
        }

        private void PerformEndOfTurnActions() {
            CoreSignals.TurnEnded.OnNext(ActivePlayer);
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
