﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation {

    public class WorkerSlot : IWorkerSlot {

        #region instance fields and properties

        #region from IWorkerSlot

        public ResourceSummary BaseYield {
            get { return _baseYield; }
        }
        private ResourceSummary _baseYield;

        public bool IsOccupied { get; set; }

        public bool IsOccupiable { get; set; }

        #endregion

        #endregion

        #region constructors

        public WorkerSlot(ResourceSummary baseYield) {
            _baseYield = baseYield;
        }

        #endregion
        
    }

}
