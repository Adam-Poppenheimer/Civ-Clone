using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Players {

    public interface IBrainPile {

        #region properties

        IPlayerBrain HumanBrain     { get; }
        IPlayerBrain BarbarianBrain { get; }

        IEnumerable<IPlayerBrain> AllBrains { get; }

        #endregion

    }

}
