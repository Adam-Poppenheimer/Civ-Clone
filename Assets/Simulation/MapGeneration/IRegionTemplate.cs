using System;
using System.Collections.Generic;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionTemplate {

        #region properties

        int HillsPercentage     { get; }
        int MountainsPercentage { get; }
        int TreePercentage      { get; }
        int RiverPercentage     { get; }
        int TundraPercentage    { get; }
        int SnowPercentage      { get; }

        int  MinTreeClumps  { get; }
        int  MaxTreeClumps  { get; }
        bool AreTreesJungle { get; }

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

        float StrategicNodesPerCell  { get; }
        float StrategicCopiesPerCell { get; }

        bool BalanceResources { get; }

        float MinFoodPerCell       { get; }
        float MinProductionPerCell { get; }

        float MinScorePerCell { get; }
        float MaxScorePerCell { get; }

        #endregion

        #region methods

        TerrainData GetNonArcticTerrainData(CellTerrain terrain);

        int GetWeightForBalanceStrategy(IBalanceStrategy strategyToCheck);

        int GetTreePlacementCostForTerrain(CellTerrain terrain);
        int GetTreePlacementCostForShape  (CellShape   shape);

        int GetSelectionWeightOfResource(IResourceDefinition resource);

        #endregion

    }

}