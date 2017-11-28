﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Distribution {

    public interface IWorkerDistributionLogic {

        #region methods

        void DistributeWorkersIntoSlots(
            int workerCount, IEnumerable<IWorkerSlot> slots,
            ICity sourceCity, DistributionPreferences preferences
        );

        #endregion

    }

}