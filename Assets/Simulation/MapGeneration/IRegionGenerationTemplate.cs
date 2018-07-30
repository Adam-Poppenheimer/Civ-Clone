using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionGenerationTemplate {

        #region properties

        int HillsPercentage     { get; }
        int MountainsPercentage { get; }
        int TreePercentage      { get; }
        int RiverPercentage     { get; }

        int MinTreeClumps { get; }
        int MaxTreeClumps { get; }

        float JungleThreshold { get; }

        float MarshChanceBase { get; }
        float MarshChancePerAdjacentRiver { get; }
        float MarshChancePerAdjacentWater { get; }

        bool HasPrimaryResource  { get; }
        int PrimaryResourceCount { get; }

        bool HasSecondaryResource  { get; }
        int SecondaryResourceCount { get; }

        bool HasTertiaryResource  { get; }
        int TertiaryResourceCount { get; }

        bool HasQuaternaryResource  { get; }
        int QuaternaryResourceCount { get; }

        #endregion

        #region methods

        TerrainData GetTerrainData(CellTerrain terrain);

        #endregion

    }

}