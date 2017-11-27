﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Production {

    public interface IProductionLogic {

        #region methods

        int GetProductionProgressPerTurnOnProject(ICity city, IProductionProject project);

        int GetGoldCostToHurryProject(ICity city, IProductionProject project);

        #endregion

    }

}
