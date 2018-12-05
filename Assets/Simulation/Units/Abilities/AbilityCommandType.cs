using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public enum AbilityCommandType {
        BuildImprovement    = 0,
        FoundCity           = 1,
        BuildRoad           = 2,
        ClearVegetation     = 3,
        SetUpToBombard      = 4,
        Fortify             = 5,
        Pillage             = 6,
        GainFreeTech        = 7,
        HurryProduction     = 8,
        StartGoldenAge      = 9,
        RepairAdjacentShips = 10
    }

}
