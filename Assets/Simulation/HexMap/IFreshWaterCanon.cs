﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IFreshWaterCanon {

        #region methods

        bool DoesCellHaveAccessToFreshWater(IHexCell cell);

        #endregion

    }

}