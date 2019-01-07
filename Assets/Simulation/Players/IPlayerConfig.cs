using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Players {

    public interface IPlayerConfig {

        #region properties

        int UnitMaxInfluenceRadius { get; }

        float WanderSelectionWeight_Distance { get; }
        float WanderSelectionWeight_Allies   { get; }

        #endregion

    }

}
