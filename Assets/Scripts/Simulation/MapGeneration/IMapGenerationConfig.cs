using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerationConfig {

        #region properties

        bool UseFixedSeed { get; }
        int  FixedSeed    { get; }

        int LandPercentage { get; }


        float LowTemperature  { get; }
        float HighTemperature { get; }

        HemisphereMode Hemispheres { get; }


        IMapTemplate TestTemplate { get; }

        int MinStrategicCopies { get; }
        int MaxStrategicCopies { get; }

        YieldSummary YieldScoringWeights { get; }


        int MaxLakeSize { get; }


        int BaseTerrainWeight          { get; }
        int TerrainTemperatureWeight   { get; }
        int TerrainPrecipitationWeight { get; }


        int RiverEndpointOnDesertWeight { get; }
        int RiverEndpointOnArcticWeight { get; }



        ReadOnlyCollection<IMapSizeCategory> MapSizes { get; }

        IMapSizeCategory DefaultMapSize { get; }

        #endregion

        #region methods

        float GetIdealTemperatureForTerrain  (CellTerrain terrain);
        float GetIdealPrecipitationForTerrain(CellTerrain terrain);

        int GetLandPercentageForSeaLevel(SeaLevelCategory seaLevel);

        #endregion

    }

}