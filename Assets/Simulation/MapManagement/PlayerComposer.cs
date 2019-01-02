using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Players;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;

namespace Assets.Simulation.MapManagement {

    public class PlayerComposer : IPlayerComposer {

        #region instance fields and properties

        private IPlayerFactory       PlayerFactory;
        private ICivilizationFactory CivFactory;
        private IGameCore            GameCore;

        #endregion

        #region constructors

        [Inject]
        public PlayerComposer(
            IPlayerFactory playerFactory, ICivilizationFactory civFactory, IGameCore gameCore
        ) {
            PlayerFactory = playerFactory;
            CivFactory    = civFactory;
            GameCore      = gameCore;
        }

        #endregion

        #region instance methods

        #region from IPlayerComposer

        public void ClearRuntime() {
            foreach(var player in PlayerFactory.AllPlayers.ToArray()) {
                PlayerFactory.DestroyPlayer(player);
            }
        }

        public void ComposePlayers(SerializableMapData mapData) {
            mapData.Players = new List<SerializablePlayerData>();

            foreach(var player in PlayerFactory.AllPlayers) {
                var playerData = new SerializablePlayerData() {
                    ControlledCiv = player.ControlledCiv.Template.Name,
                    Brain         = player.Brain.Name
                };

                mapData.Players.Add(playerData);
            }

            mapData.ActivePlayer = GameCore.ActivePlayer != null ? GameCore.ActivePlayer.Name : null;
        }

        public void DecomposePlayers(SerializableMapData mapData) {
            foreach(var playerData in mapData.Players) {
                var controlledCiv = CivFactory.AllCivilizations.FirstOrDefault(
                    civ => civ.Template.Name.Equals(playerData.ControlledCiv)
                );

                if(controlledCiv == null) {
                    throw new InvalidOperationException("Could not find a civ of template " + playerData.ControlledCiv);
                }

                var playerBrain = PlayerFactory.AllBrains.FirstOrDefault(
                    brain => brain.Name.Equals(playerData.Brain)
                );

                if(playerBrain == null) {
                    throw new InvalidOperationException("Could not find a brain of name " + playerData.Brain);
                }

                PlayerFactory.CreatePlayer(controlledCiv, playerBrain);
            }

            GameCore.ActivePlayer = mapData.ActivePlayer != null
                ? PlayerFactory.AllPlayers.First(player => player.Name.Equals(mapData.ActivePlayer))
                : null;
        }

        #endregion

        #endregion
        
    }

}
