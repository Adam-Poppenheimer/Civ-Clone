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

        bool HasPrimaryLuxury  { get; }
        int PrimaryLuxuryCount { get; }

        bool HasSecondaryLuxury  { get; }
        int SecondaryLuxuryCount { get; }

        bool HasTertiaryLuxury  { get; }
        int TertiaryLuxuryCount { get; }

        bool HasQuaternaryLuxury  { get; }
        int QuaternaryLuxuryCount { get; }

        bool BalanceResources { get; }

        float MinFoodPerCell       { get; }
        float MinProductionPerCell { get; }

        float MinScorePerCell { get; }
        float MaxScorePerCell { get; }

        #endregion

        #region methods

        TerrainData GetTerrainData(CellTerrain terrain);

        #endregion

    }

}