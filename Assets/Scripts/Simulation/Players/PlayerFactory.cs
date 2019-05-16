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

        #endregion

        private PlayerSignals PlayerSignals;

        #endregion

        #region constructors

        [Inject]
        public PlayerFactory(PlayerSignals playerSignals) {
            PlayerSignals = playerSignals;
        }

        #endregion

        #region instance methods

        #region from IPlayerFactory

        public IPlayer CreatePlayer(ICivilization civ, IPlayerBrain brain) {
            var newPlayer = new Player(civ, brain);

            allPlayers.Add(newPlayer);

            allPlayers.Sort(PlayerSorter);

            PlayerSignals.PlayerCreated.OnNext(newPlayer);

            return newPlayer;
        }

        public void DestroyPlayer(IPlayer player) {
            player.Clear();

            allPlayers.Remove(player);

            allPlayers.Sort(PlayerSorter);

            PlayerSignals.PlayerBeingDestroyed.OnNext(player);
        }

        #endregion

        private int PlayerSorter(IPlayer playerOne, IPlayer playerTwo) {
            bool oneIsBarbaric = playerOne.ControlledCiv.Template.IsBarbaric;
            bool TwoIsBarbaric = playerTwo.ControlledCiv.Template.IsBarbaric;

            if(oneIsBarbaric) {
                if(TwoIsBarbaric) {
                    return playerOne.Name.CompareTo(playerTwo.Name);
                }else {                    
                    return 1;
                }
            }else if(TwoIsBarbaric) {
                return -1;
            }else {
                return playerOne.Name.CompareTo(playerTwo.Name);
            }
        }

        #endregion

    }

}
