﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.AI {

    public interface IUnitStrengthEstimator {

        #region methods

        float EstimateUnitStrength(IUnit unit);

        #endregion

    }

}
