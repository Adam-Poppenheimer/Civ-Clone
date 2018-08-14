using System;
using System.Collections.Generic;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerationConfig {

        #region properties

        bool UseFixedSeed { get; }
        int  FixedSeed    { get; }

        int LandPercentage { get; }


        int HardMapBorderX { get; }
        int HardMapBorderZ { get; }

        int SoftMapBorderX { get; }
        int SoftMapBorderZ { get; }

        int SoftBorderAvoidanceWeight { get; }


        float LowTemperature  { get; }
        float HighTemperature { get; }

        HemisphereMode Hemispheres { get; }

        float TemperatureJitter { get; }


        IMapGenerationTemplate MapTemplate { get; }

        int MinStrategicCopies { get; }
        int MaxStrategicCopies { get; }

        YieldSummary YieldScoringWeights { get; }


        int MaxLakeSize { get; }

        #endregion

    }

}