using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Promotions {

    public interface IUnitPromotionLogic {

        #region methods

        IEnumerable<IPromotion> GetPromotionsForUnit(IUnit unit);

        #endregion

    }

}
