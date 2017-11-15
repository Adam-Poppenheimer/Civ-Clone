using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public class PopulationGrowthLogic : IPopulationGrowthLogic {

        #region instance methods

        #region from IPopulationGrowthLogic

        public int GetFoodConsumptionPerTurn(ICity city) {
            throw new NotImplementedException();
        }

        public int GetFoodStockpileAfterGrowth(ICity city) {
            throw new NotImplementedException();
        }

        public int GetFoodStockpileAfterStarvation(ICity city) {
            throw new NotImplementedException();
        }

        public int GetFoodStockpileToGrow(ICity city) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
        
    }

}
