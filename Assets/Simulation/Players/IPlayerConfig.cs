using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Players {

    public interface IPlayerConfig {

        #region properties

        IPlayerBrain HumanBrain     { get; }
        IPlayerBrain BarbarianBrain { get; }

        #endregion

    }

}
