using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.WorkerSlots {

    public class WorkerSlotFactory : IWorkerSlotFactory {

        #region instance fields and properties

        private DiContainer Container;

        #endregion

        #region constructors

        [Inject]
        public WorkerSlotFactory(DiContainer container) {
            Container = container;
        }

        #endregion

        #region instance methods

        #region from IWorkerSlotFactory

        public IWorkerSlot BuildSlot(ResourceSummary baseYield) {
            var newSlot = Container.Instantiate<WorkerSlot>();

            newSlot.BaseYield = baseYield;

            return newSlot;
        }

        #endregion

        #endregion

    }

}
