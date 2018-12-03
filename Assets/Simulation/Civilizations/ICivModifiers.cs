using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface ICivModifiers {

        #region properties

        ICivModifier<bool> SuppressGarrisonMaintenance { get; }

        ICivModifier<float> ImprovementBuildSpeed { get; }

        ICivModifier<float> GreatMilitaryGainSpeed { get; }

        #endregion

    }

}
