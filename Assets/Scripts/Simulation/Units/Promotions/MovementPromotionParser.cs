using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Promotions {

    public class MovementPromotionParser : IMovementPromotionParser {

        #region instance fields and properties



        #endregion

        #region constructors



        #endregion

        #region

        #region from IMovementPromotionParser

        public void AddPromotionToMovementSummary(
            IPromotion promotion, UnitMovementSummary summary
        ) {
            summary.CanTraverseLand         |= promotion.PermitsLandTraversal;
            summary.CanTraverseShallowWater |= promotion.PermitsShallowWaterTraversal;
            summary.CanTraverseDeepWater    |= promotion.PermitsDeepWaterTraversal;

            summary.BonusMovement += promotion.BonusMovement;
            summary.BonusVision   += promotion.BonusVision;

            foreach(var terrain in promotion.TerrainsWithIgnoredCosts) {
                summary.TerrainsWithIgnoredCosts.Add(terrain);
            }

            foreach(var shape in promotion.ShapesWithIgnoredCosts) {
                summary.ShapesWithIgnoredCosts.Add(shape);
            }

            foreach(var vegetation in promotion.VegetationsWithIgnoredCosts) {
                summary.VegetationsWithIgnoredCosts.Add(vegetation);
            }


            foreach(var shapes in promotion.ShapesConsumingFullMovement) {
                summary.ShapesConsumingFullMovement.Add(shapes);
            }

            foreach(var vegetation in promotion.VegetationsConsumingFullMovement) {
                summary.VegetationConsumingFullMovement.Add(vegetation);
            }
        }

        #endregion

        #endregion
        
    }

}
