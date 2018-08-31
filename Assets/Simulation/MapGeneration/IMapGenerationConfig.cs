﻿using System;
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


        IMapTemplate MapTemplate { get; }

        int MinStrategicCopies { get; }
        int MaxStrategicCopies { get; }

        YieldSummary YieldScoringWeights { get; }


        int MaxLakeSize { get; }


        int BaseTerrainWeight          { get; }
        int TerrainTemperatureWeight   { get; }
        int TerrainPrecipitationWeight { get; }


        int RiverEndpointOnDesertWeight { get; }
        int RiverEndpointOnArcticWeight { get; }

        #endregion

        #region methods

        float GetIdealTemperatureForTerrain  (CellTerrain terrain);
        float GetIdealPrecipitationForTerrain(CellTerrain terrain);

        #endregion

    }

}