using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Players {

    public class PlayerFactory : IPlayerFactory {

        #region instance fields and properties

        #region from IPlayerFactory

        public ReadOnlyCollection<IPlayer> AllPlayers {
            get { return allPlayers.AsReadOnly(); }
        }
        private List<IPlayer> allPlayers = new List<IPlayer>();

        public IEnumerable<IPlayerBrain> AllBrains {
            get { return _allBrains; }
        }
        private List<IPlayerBrain> _allBrains;

        #endregion



        private PlayerSignals PlayerSignals;

        #endregion

        #region constructors

        [Inject]
        public PlayerFactory(PlayerSignals playerSignals, List<IPlayerBrain> allBrains) {
            PlayerSignals = playerSignals;
            _allBrains    = allBrains;
        }

        #endregion

        #region instance methods

        #region from IPlayerFactory

        public IPlayer CreatePlayer(ICivilization civ, IPlayerBrain brain) {
            var newPlayer = new Player(civ, brain);

            allPlayers.Add(newPlayer);

            PlayerSignals.PlayerCreated.OnNext(newPlayer);

            return newPlayer;
        }

        public void DestroyPlayer(IPlayer player) {
            allPlayers.Remove(player);

            PlayerSignals.PlayerBeingDestroyed.OnNext(player);
        }

        #endregion

        #endregion

    }

}
