using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface ITileConfig {

        #region properties

        Material DesertMaterial    { get; }
        Material GrasslandMaterial { get; }
        Material PlainsMaterial    { get; }

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

        #region methods

        Material GetTerrainMaterial(TerrainType terrain);

        #endregion

    }

}