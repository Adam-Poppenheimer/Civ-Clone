﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Growth {

    public interface IPopulationGrowthLogic  {

        #region methods

        int GetFoodConsumptionPerTurn(ICity city);
        int GetFoodStockpileToGrow(ICity city);

        int GetFoodStockpileSubtractionAfterGrowth(ICity city);
        int GetFoodStockpileAfterStarvation(ICity city);

        #endregion

    }

}
