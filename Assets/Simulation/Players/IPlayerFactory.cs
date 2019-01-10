using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Players {

    public interface IPlayerFactory {

        #region properties

        ReadOnlyCollection<IPlayer> AllPlayers { get; }

        #endregion

        #region methods

        IPlayer CreatePlayer(ICivilization civ, IPlayerBrain brain);

        void DestroyPlayer(IPlayer player);

        #endregion

    }

}
