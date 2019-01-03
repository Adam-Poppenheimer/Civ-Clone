using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Players {

    public interface IPlayer {

        #region properties

        string Name { get; }

        ICivilization ControlledCiv { get; }

        IPlayerBrain Brain { get; }

        #endregion

        #region methods

        void PassControl(Action controlRelinquisher);

        void Clear();

        #endregion

    }

}
