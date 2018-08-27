﻿using System;
using System.Collections.Generic;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionBiomeTemplate {

        #region properties

        float MinTemperature { get; }
        float MaxTemperature { get; }

        float MinPrecipitation { get; }
        float MaxPrecipitation { get; }

        TerrainData GrasslandData { get; }
        TerrainData PlainsData    { get; }
        TerrainData DesertData    { get; }

        int TreePercentage   { get; }
        int RiverPercentage  { get; }
        int TundraPercentage { get; }
        int SnowPercentage   { get; }

        int  MinTreeClumps  { get; }
        int  MaxTreeClumps  { get; }
        bool AreTreesJungle { get; }

        float MarshChanceBase             { get; }
        float MarshChancePerAdjacentRiver { get; }
        float MarshChancePerAdjacentWater { get; }

        IEnumerable<RegionBalanceStrategyData> BalanceStrategyWeights { get; }
        IEnumerable<RegionResourceData>        ResourceWeights        { get; }

        #endregion

        #region methods

        int GetTreePlacementCostForTerrain(CellTerrain terrain);
        int GetTreePlacementCostForShape  (CellShape   shape);

        #endregion

    }

}