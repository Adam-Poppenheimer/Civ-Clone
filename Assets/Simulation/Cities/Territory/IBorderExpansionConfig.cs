using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Territory {

    public interface IBorderExpansionConfig {

        #region properties

        int MaxRange { get; }

        int TileCostBase { get; }
        int PreviousTileCountCoefficient { get; }
        float PreviousTileCountExponent { get; }

        #endregion

    }

}
