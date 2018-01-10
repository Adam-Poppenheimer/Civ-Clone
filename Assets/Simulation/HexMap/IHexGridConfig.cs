﻿using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexGridConfig {

        #region properties

        int RandomSeed { get; }

        ReadOnlyCollection<Color> ColorsOfTerrains { get; }

        ReadOnlyCollection<int> ElevationsOfShapes { get; }

        ResourceSummary DesertYield     { get; }
        ResourceSummary ForestYield     { get; }
        ResourceSummary GrasslandYield { get; }
        ResourceSummary HillsYield      { get; }
        ResourceSummary PlainsYield     { get; }

        int GrasslandMoveCost    { get; }
        int PlainsMoveCost       { get; }
        int DesertMoveCost       { get; }
        int HillsMoveCost        { get; }
        int ForestMoveCost       { get; }
        int ShallowWaterMoveCost { get; }
        int DeepWaterMoveCost    { get; }

        ReadOnlyCollection<TerrainType> UnoccupiableTerrains { get; }

        #endregion

    }

}