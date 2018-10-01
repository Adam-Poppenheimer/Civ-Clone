using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public class UnitMovementSummary : IUnitMovementSummary {

        #region instance fields and properties

        #region from IUnitMovementSummary

        public bool CanTraverseLand         { get; set; }
        public bool CanTraverseShallowWater { get; set; }
        public bool CanTraverseDeepWater    { get; set; }

        public int BonusMovement { get; set; }
        public int BonusVision   { get; set; }

        #endregion

        public HashSet<CellShape>      ShapesConsumingFullMovement     = new HashSet<CellShape>();
        public HashSet<CellVegetation> VegetationConsumingFullMovement = new HashSet<CellVegetation>();

        public HashSet<CellTerrain>    TerrainsWithIgnoredCosts    = new HashSet<CellTerrain>();
        public HashSet<CellShape>      ShapesWithIgnoredCosts      = new HashSet<CellShape>();
        public HashSet<CellVegetation> VegetationsWithIgnoredCosts = new HashSet<CellVegetation>();

        #endregion

        #region instance methods

        #region from IUnitMovementSummary

        public bool DoesShapeConsumeFullMovement(CellShape shape) {
            return ShapesConsumingFullMovement.Contains(shape);
        }

        public bool DoesVegetationConsumeFullMovement(CellVegetation vegetation) {
            return VegetationConsumingFullMovement.Contains(vegetation);
        }

        public bool IsCostIgnoredOnTerrain(CellTerrain terrain) {
            return TerrainsWithIgnoredCosts.Contains(terrain);
        }

        public bool IsCostIgnoredOnShape(CellShape shape) {
            return ShapesWithIgnoredCosts.Contains(shape);
        }

        public bool IsCostIgnoredOnVegetation(CellVegetation vegetation) {
            return VegetationsWithIgnoredCosts.Contains(vegetation);
        }

        #endregion

        public void Reset() {
            CanTraverseLand         = false;
            CanTraverseShallowWater = false;
            CanTraverseDeepWater    = false;

            BonusMovement = 0;
            BonusVision   = 0;

            ShapesConsumingFullMovement    .Clear();
            VegetationConsumingFullMovement.Clear();

            TerrainsWithIgnoredCosts   .Clear();
            ShapesWithIgnoredCosts     .Clear();
            VegetationsWithIgnoredCosts.Clear();
        }

        #endregion

    }

}
