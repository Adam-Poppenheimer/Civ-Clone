using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public interface IUnitMovementSummary {

        #region properties

        bool CanTraverseDeepWater    { get; }
        bool CanTraverseLand         { get; }
        bool CanTraverseShallowWater { get; }

        int BonusMovement { get; }
        int BonusVision   { get; }

        #endregion

        #region methods

        bool DoesShapeConsumeFullMovement     (CellShape      shape);
        bool DoesVegetationConsumeFullMovement(CellVegetation vegetation);

        bool IsCostIgnoredOnTerrain   (CellTerrain    terrain);
        bool IsCostIgnoredOnShape     (CellShape      shape);
        bool IsCostIgnoredOnVegetation(CellVegetation vegetation);

        #endregion

    }

}