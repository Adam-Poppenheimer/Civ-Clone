using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation {

    public class WorkerSlot : IWorkerSlot {

        #region instance fields and properties

        #region from IWorkerSlot

        public ResourceSummary BaseYield { get; set; }

        public bool IsOccupied { get; set; }

        public bool IsLocked { get; set; }

        #endregion

        #endregion

        #region constructors

        public WorkerSlot(ResourceSummary baseYield) {
            BaseYield = baseYield;
        }

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format("Slot: {0}", BaseYield);
        }

        #endregion

        #endregion

    }

}
