﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    public interface IHappinessLogic {

        #region methods

        int GetHappinessOfCity(ICity city);

        #endregion

    }

}
