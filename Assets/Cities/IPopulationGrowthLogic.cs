using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface IPopulationGrowthLogic  {

        #region methods

        int GetFoodConsumptionPerTurn(ICity city);
        int GetFoodStockpileToGrow(ICity city);

        int GetFoodStockpileAfterGrowth(ICity city);
        int GetFoodStockpileAfterStarvation(ICity city);

        #endregion

    }

}
